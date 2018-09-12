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
                 (x => x.Name == "table" && x.HasClass("tablesaw"))
                 .Where(x => x.Descendants("td").Count() > 4);


            foreach (var tb in accs.Where(s => s.Descendants("tbody").Count() > 0))
            {
                var header = tb.Descendants("thead")
                    .Where(x => x.Descendants("th").Count() > 4).FirstOrDefault()?
                    .Descendants("th")
                    .Select((x, i) => new KeyValuePair<int, string>(i, x.GetAttributeValue("title", null)))
                    .ToDictionary(x => x.Key, x => x.Value);
                foreach (var i in tb.Descendants("tbody").FirstOrDefault()?.Descendants("tr"))
                {
                    var cells = i.Descendants("td");
                    string proc(string a) => WebUtility.HtmlDecode(a).Trim();
                    if (cells.Count() < 4) continue;
                    int p = 0;

                    string SetAndGoNext() => proc(cells.ElementAt(p++).InnerText);
                    bool IsMatch(string ename) => header[p].ToLower().Contains(ename);
                    string SplitCellToMap() => proc(string.Join('\n', (cells.ElementAt(p++).InnerHtml.Split("<br>", StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => Regex.Replace(x, @"<(?:\/|).*?>", "")))));

                    var id = int.Parse(SetAndGoNext()); //const anywhere
                    var name = SetAndGoNext(); //const also

                    // collumns are not on same indexes
                    // so we do the trick with headers starting from 2 index
                    var priority = IsMatch("пріоритет") && (Degree != Institute.Degree.Magister)
                        ? SetAndGoNext() : null;
                    var status = !IsMatch("статус") ? null : (SetAndGoNext());
                    bool prioSet = IsMatch("пріоритет");
                    var contMark = prioSet ? null : SetAndGoNext();
                    priority = !prioSet ? priority : SetAndGoNext();
                    contMark = IsMatch("конкурсний бал") ? SetAndGoNext() : contMark;
                    contMark = IsMatch("конкурсний бал") ? SetAndGoNext() : contMark;
                    status = IsMatch("статус") ? SetAndGoNext() : status;
                    var details = IsMatch("детал") ? SplitCellToMap() : null;
                    details = IsMatch("детал") ? SplitCellToMap() : details;
                    var coef = IsMatch("коефіц") ? SplitCellToMap() : null;
                    var quote = IsMatch("квот") ? SetAndGoNext() : null;
                    var origs = SetAndGoNext() == "+";

                    Students.Add(new Student()
                    {
                        Id = id,
                        Name = name,
                        Status = status,
                        Priority = priority,
                        ContestMark = contMark,
                        Detail = details,
                        Quote = quote,
                        Origs = origs
                    });
                }
            }
        }

        public Specialty SetType(string raw)
        {
            return this;
        }
    }

    public partial class Student
    {
        public string Status { get; internal set; }
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
                        var namex = map.Where(x => x.Key.ToLower()
                        .Contains("освітня прог".ToLower())).FirstOrDefault();
                        if (string.IsNullOrEmpty(namex.Key))
                        {
                            namex = map.Where(x => x.Key.ToLower()
                            .Contains("спеціальн".ToLower())).FirstOrDefault();
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
