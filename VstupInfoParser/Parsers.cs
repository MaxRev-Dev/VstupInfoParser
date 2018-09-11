using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VstupInfoParser.Models_JSON
{

    #region Specialties

    public partial class Specialty : Instance
    {
        public Specialty(string name, int gID,
            Institute.Degree degree,
            Institute.StudyType type,
            string url, int year,
            IEnumerable<KeyValuePair<string, string>> map) : base(name, url, year)
        {
            GlobalID = gID;
            Type = type;
            Map = map;
            Degree = degree;
        }
        protected override void Parse(HtmlDocument doc)
        {
            var accs = doc.DocumentNode.Descendants().Where
                 (x => x.Name == "table" && x.HasClass("tablesaw")).FirstOrDefault();

            foreach (var i in accs.Descendants("tbody").First().Descendants("tr"))
            {
                var cells = i.Descendants("td");
                string proc(string a) => WebUtility.HtmlDecode(a).Trim();
                if (cells.Count() < 4) continue;
                var id = int.Parse(proc(cells.ElementAt(0).InnerText));
                var name = proc(cells.ElementAt(1).InnerText);
                var priority = proc(cells.ElementAt(2).InnerText);
                var cb = proc(cells.ElementAt(3).InnerText);
                var state = proc(cells.ElementAt(4).InnerText);
                var detail = proc(cells.ElementAt(5).InnerText);
                var quote = proc(cells.ElementAt(6).InnerText) == "+";
                var origs = proc(cells.ElementAt(7).InnerText) == "+";
                Students.Add(new Students(id, name, priority, cb, state, detail, quote, origs));
            }
        }

        public Specialty SetType(string raw)
        {
            return this;
        }
    }

    public partial class Students
    {
        public Students(int id, string name, string priority, string cb,
            string state, string detail, bool quote, bool origs)
        {
            Id = id;
            Name = name;
            Priority = priority;
            ContestMark = cb;
            State = state;
            Detail = detail;
            Quote = quote;
            Origs = origs;
        }
    }

    #endregion

    #region Institutes 
    public partial class Institute : Instance
    {
        public enum InstanceType
        {
            Univer,
            Academy,
            Institute,
            College,
            Tech,
            Other
        }
        public enum StudyType { Full, Part }
        public enum Degree { Bachelor, Magister }

        private Dictionary<string, InstanceType> Pairs { get; } = new Dictionary<string, InstanceType>()
        {
            {"акад",InstanceType.Academy  },
            {"інстит",InstanceType.Institute  },
            {"коле",InstanceType.College  },
            {"відо",InstanceType.Other  },
            {"техн",InstanceType.Tech  },
            {"унів",InstanceType.Univer  },
        };
        public Institute SetType(string raw)
        {
            Type = Pairs.Where(x => raw.ToLower(new CultureInfo("uk-UA")).Contains(x.Key)).First().Value;
            return this;
        }

        public Institute(string name, string url, int year) : base(name, url, year)
        {
        }
        private Dictionary<string, StudyType> _forms { get; } = new Dictionary<string, StudyType>()
        {
            {"денна", StudyType.Full },
            { "заочна",StudyType.Part }
        };
        private Dictionary<string, Degree> _degrees { get; } = new Dictionary<string, Degree>()
        {
            {"бакала", Degree.Bachelor },
            { "магіс",Degree.Magister }
        };
        protected override void Parse(HtmlDocument doc)
        {
            var accs = doc.DocumentNode.Descendants().Where
                (x => x.Name == "div" && x.HasClass("accordion-group"));

            foreach (var i in accs)
            {
                var q = i.Descendants("div").Where(x => x.HasClass("tabbable"));
                if (q.Any() == false)
                    continue;
                var form = i.Descendants("div").Where(x => x.HasClass("accordion-heading")).First();
                var stype = form.Descendants("a").First().InnerText;

                var stypev = _forms.Where(x => stype.ToLower().Contains(x.Key)).First().Value;

                var tables = q.First();
                var tabs = tables.Descendants("ul").Where(x => x.HasClass("nav") && x.HasClass("nav-tabs")).First();
                var types = tabs.Descendants("a").Select(x => new { Name = x.InnerText, Link = x.GetAttributeValue("href", null)?.Trim('#') });

                foreach (var t in tables.Descendants("div").Where(x => x.HasClass("tab-pane")))
                {
                    var id = t.GetAttributeValue("id", null);
                    var proj = types.Where(x => x.Link.Equals(id)).First();
                    var type = _degrees.Where(x => proj.Name.ToLower().Contains(x.Key)).First().Value;

                    foreach (var tr in t.Descendants("tbody").First().Descendants("tr"))
                    {
                        var m = tr.Descendants("td").First()
                            .InnerHtml.Split("<br>", StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => Regex.Replace(x, @"<(?:\/|).*?>", ""))
                            .Select(x => WebUtility.HtmlDecode(x).Trim())
                            .Where(x => !string.IsNullOrEmpty(x));
                        var map = m.Select(x => x.Split(':')).Select(x =>
                             x.Length > 1 ? new KeyValuePair<string, string>(x[0].Trim(), x[1].Trim()) :
                              new KeyValuePair<string, string>(x[0].Trim(), ""));

                        var abtn = tr.Descendants("td").ElementAt(1).Descendants("a").FirstOrDefault();
                        var link = abtn == default ? null : '/' + Year.ToString() + abtn?.GetAttributeValue("href", null).Trim('.');
                        if (!Specialties.ContainsKey(stypev))
                            Specialties[stypev] = new List<Specialty>();
                        var namex = map.Where(x => x.Key.ToLower(new CultureInfo("uk-UA"))
                        .Contains("Освітня прог".ToLower(new CultureInfo("uk-UA")))).FirstOrDefault();
                        if (string.IsNullOrEmpty(namex.Key))
                        {
                            namex = map.Where(x => x.Key.ToLower(new CultureInfo("uk-UA"))
                        .Contains("Спеціальн".ToLower(new CultureInfo("uk-UA")))).FirstOrDefault();
                        } 
                        var g = link?.Substring(link.LastIndexOf('p') + 1).Replace(".html", "");
                        var gID = link == default ? -1 : int.Parse(g);
                        if (link == null) continue;
                        Specialties[stypev].Add(new Specialty(namex.Equals(default) ? null : namex.Value, gID, type, stypev,
                             CoreParser.Current.FromBase(link), Year, map));
                    }

                }
            }


        }
    }

    #endregion

    #region Regions
    public partial class Region : Instance
    {
        public Region(string name, string url) : base(name, url, -1)
        {
        }

        protected override void Parse(HtmlDocument doc)
        {
            var tables = doc.DocumentNode.Descendants().Where
                (x => x.Name == "div" && x.HasClass("accordion-group"));

            foreach (var t in tables)
            {
                var name = t.Descendants("a").First().InnerText;
                foreach (var td in t.Descendants("td"))
                {
                    foreach (var a in td.Descendants("a").Where(x => x.GetAttributeValue("target", null) == null))
                    {
                        var href = a.Attributes["href"].Value;
                        string stryear = href.Substring(3, href.IndexOf('i', 4) - 3);
                        int year = int.Parse(stryear);
                        string strregion = href.Substring(href.IndexOf(stryear, 3) + 5).Replace(".html", "");
                        int region = int.Parse(strregion);

                        Institutes.Add(new Institute(a.InnerText,
                            CoreParser.Current.FromBase('/' + year.ToString() + href.Trim('.')), year).SetType(name));
                    }
                }
            }

        }
    }
    public class RegionTable
    {
        public Instance this[int index]
        {
            get => Regions[index];
            set => Regions[index] = value;
        }
        public Dictionary<int, Instance> Regions { get; } = new Dictionary<int, Instance>();
    }
    internal class DynamicRegionTable
    {
        public Dictionary<int, RegionTable> Years { get; } = new Dictionary<int, RegionTable>();
        private string BaseUrl { get; }
        public DynamicRegionTable(string baseUrl)
        {
            BaseUrl = baseUrl;
        }
        public Exception OnProcessError { get; private set; }
        public async Task Fetch()
        {
            try
            {
                HttpClient client = new HttpClient();
                var docStream = await client.GetStreamAsync(BaseUrl);

                HtmlDocument doc = new HtmlDocument();
                doc.Load(docStream);

                var tables = doc.DocumentNode.Descendants().Where
                    (x => x.Name == "table" && x.HasClass("tablesaw") && x.HasClass("tablesaw-stack"));

                foreach (var t in tables)
                {
                    RegionTable regionTable = default;
                    foreach (var a in t.Descendants("a").Where(x => x.GetAttributeValue("target", null) == null))
                    {
                        var href = a.Attributes["href"].Value;
                        string stryear = href.Substring(1, href.IndexOf('/', 2) - 1);
                        int year = int.Parse(stryear);
                        string strregion = href.Substring(href.IndexOf(stryear, 3) + 5).Replace(".html", "");
                        int region = int.Parse(strregion);
                        if (regionTable == default)
                        {
                            Years[year] = regionTable = new RegionTable();
                        }
                        regionTable[region] = new Region(a.InnerText, CoreParser.Current.FromBase(href));
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessError = ex;
            }
        }
    }
    #endregion

    public abstract partial class Instance
    {
        public Instance(string name, string url, int year)
        {
            Name = name; Url = url; Year = year;
        }
        public override string ToString()
        {
            return Name + ": " + Url;
        }
        protected abstract void Parse(HtmlDocument doc);
        public async Task Fetch()
        {
            if (Fetched != default) return;
            Fetched = DateTime.Now;
            try
            {
                HttpClient client = new HttpClient();
                var docStream = await client.GetStreamAsync(Url);

                HtmlDocument doc = new HtmlDocument();
                doc.Load(docStream);
                Parse(doc);
            }
            catch (Exception ex)
            {
                OnProcessError = ex;
            }
        }
    }
}
