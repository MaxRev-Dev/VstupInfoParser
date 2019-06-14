using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using VstupInfoParser.ModelsJSON;

namespace VstupInfoParser.Parsers
{
    internal class DynamicRegionTable
    {
        public Dictionary<int, RegionTable> Years { get; } = new Dictionary<int, RegionTable>();
        private string BaseUrl { get; }
        public DynamicRegionTable(string baseUrl)
        {
            BaseUrl = baseUrl;
        }
        public Exception OnProcessError { get; private set; }
        public async Task FetchAsync()
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
                        string strregion = href.Substring(href.IndexOf(stryear, 3, StringComparison.Ordinal) + 5).Replace(".html", "");
                        int region = int.Parse(strregion);
                        if (regionTable == default)
                        {
                            Years[year] = regionTable = new RegionTable();
                        }
                        regionTable[region] = new Region(a.InnerText, CoreParser.FromBase(href));
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessError = ex;
            }
        }
    }
}