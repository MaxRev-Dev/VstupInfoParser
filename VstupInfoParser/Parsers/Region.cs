using System;
using System.Linq;
using HtmlAgilityPack;
using VstupInfoParser.Parsers;

namespace VstupInfoParser.ModelsJSON
{
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
                        string strregion = href.Substring(href.IndexOf(stryear, 3, StringComparison.Ordinal) + 5).Replace(".html", "");
                        int unused = int.Parse(strregion);

                        Institutes.Add(new Institute(a.InnerText,
                            CoreParser.FromBase('/' + year.ToString() + href.Trim('.')), year).SetType(name));
                    }
                }
            }

        }
    }
}