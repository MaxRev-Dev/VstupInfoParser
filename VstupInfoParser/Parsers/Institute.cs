using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using VstupInfoParser.Parsers;

namespace VstupInfoParser.ModelsJSON
{
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
            Type = Pairs.First(x => raw.ToLower(MainApp.DefaultCultureInfo).Contains(x.Key)).Value;
            return this;
        }

        public Institute(string name, string url, int year) : base(name, url, year)
        {
        }
        private Dictionary<string, StudyType> Forms { get; } = new Dictionary<string, StudyType>()
        {
            { "денна", StudyType.Full },
            { "заочна",StudyType.Part }
        };
        private Dictionary<string, Degree> Degrees { get; } = new Dictionary<string, Degree>()
        {
            { "бакала", Degree.Bachelor },
            { "магіс",Degree.Magister }
        };
        protected override void Parse(HtmlDocument doc)
        {
            var accs = doc.DocumentNode.Descendants().Where
                (x => x.Name == "div" && x.HasClass("accordion-group"));

            foreach (var i in accs)
            {
                var q = i.Descendants("div").Where(x => x.HasClass("tabbable")).ToArray();
                if (q.Any() == false)
                {
                    continue;
                }

                var form = i.Descendants("div").First(x => x.HasClass("accordion-heading"));
                var stype = form.Descendants("a").First().InnerText;

                var stypev = Forms.First(x => stype.ToLower().Contains(x.Key)).Value;

                var tables = q.First();
                var tabs = tables.Descendants("ul").First(x => x.HasClass("nav") && x.HasClass("nav-tabs"));
                var types = tabs.Descendants("a").Select(x => new { Name = x.InnerText, Link = x.GetAttributeValue("href", null)?.Trim('#') }).ToArray();

                foreach (var t in tables.Descendants("div").Where(x => x.HasClass("tab-pane")))
                {
                    var id = t.GetAttributeValue("id", null);
                    var proj = types.First(x => x.Link.Equals(id));
                    var type = Degrees.First(x => proj.Name.ToLower().Contains(x.Key)).Value;

                    foreach (var tr in t.Descendants("tbody").First().Descendants("tr"))
                    {
                        var m = tr.Descendants("td").First()
                            .InnerHtml.Split("<br>", StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => Regex.Replace(x, @"<(?:\/|).*?>", ""))
                            .Select(x => WebUtility.HtmlDecode(x).Trim())
                            .Where(x => !string.IsNullOrEmpty(x));
                        var map = m.Select(x => x.Split(':')).Select(x =>
                            x.Length > 1 ? new KeyValuePair<string, string>(x[0].Trim(), x[1].Trim()) :
                                new KeyValuePair<string, string>(x[0].Trim(), "")).ToArray();

                        var abtn = tr.Descendants("td").ElementAt(1).Descendants("a").FirstOrDefault();
                        var link = abtn == default ? null : '/' + Year.ToString() + abtn.GetAttributeValue("href", null).Trim('.');

                        if (!Specialties.ContainsKey(stypev))
                        {
                            Specialties[stypev] = new List<Specialty>();
                        }

                        var namex = map.FirstOrDefault(x => x.Key.ToLower()
                            .Contains("освітня прог".ToLower(CultureInfo.InvariantCulture)));
                        if (string.IsNullOrEmpty(namex.Key))
                        {
                            namex = map.FirstOrDefault(x => x.Key.ToLower()
                                .Contains("спеціальн".ToLower(CultureInfo.InvariantCulture)));
                        }
                        var g = link?.Substring(link.LastIndexOf('p') + 1).Replace(".html", "");
                        var gId = link == default ? -1 : int.Parse(g);
                        if (link == null)
                        {
                            continue;
                        }

                        Specialties[stypev].Add(new Specialty(namex.Equals(default) ? null : namex.Value, gId, type, stypev,
                            CoreParser.FromBase(link), Year, map));
                    }

                }
            }
        }
    }
}