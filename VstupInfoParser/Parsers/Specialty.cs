using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace VstupInfoParser.ModelsJSON
{
    public partial class Specialty : ModelsJSON.Instance
    {
        public Specialty(string name, int gId,
            Institute.Degree degree,
            Institute.StudyType type,
            string url, int year,
            IEnumerable<KeyValuePair<string, string>> map) : base(name, url, year)
        {
            GlobalId = gId;
            Type = type;
            Map = map;
            Degree = degree;
        }
        protected override void Parse(HtmlDocument doc)
        {
            var accs = doc.DocumentNode.Descendants().Where
                    (x => x.Name == "table" && x.HasClass("tablesaw"))
                .Where(x => x.Descendants("td").Count() > 4);


            foreach (var tb in accs.Where(s => s.Descendants("tbody").Any()))
            {
                var header = tb
                    .Descendants("thead").FirstOrDefault(x => x.Descendants("th").Count() > 4)?
                    .Descendants("th")
                    .Select((x, i) => new KeyValuePair<int, string>(i, x.GetAttributeValue("title", null)))
                    .ToDictionary(x => x.Key, x => x.Value);
                var tx = tb.Descendants("tbody").FirstOrDefault();
                if (tx != default && header != default)
                    foreach (var i in tx.Descendants("tr"))
                    {
                        var cells = i.Descendants("td").ToArray();
                        string proc(string a) => WebUtility.HtmlDecode(a)?.Trim();
                        if (cells.Length < 4) continue;
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
                        var _ = IsMatch("коефіц") ? SplitCellToMap() : null;
                        var quote = IsMatch("квот") ? SetAndGoNext() : null;
                        var origs = SetAndGoNext() == "+";

                        Students.Add(new Student
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
    }
}