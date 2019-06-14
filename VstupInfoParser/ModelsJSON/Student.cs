using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace VstupInfoParser.ModelsJSON
{
    public class Student
    {
        public string Status { get; internal set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("priority")]
        public string Priority { get; set; }
        [JsonProperty("cont_mark")]
        public string ContestMark { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("detail")]
        public string Detail { get; set; }
        [JsonProperty("quote")]
        public string Quote { get; set; }
        [JsonProperty("origs")]
        public bool Origs { get; set; }

        [JsonProperty("doc_aver")]
        public string DocumentAverage => StrOrNull("бал доку");
        [JsonProperty("spec_contest")]
        public string SpecialtyContest => StrOrNull("фахове випробув");
        [JsonProperty("foreign_lang")]
        public string ForeignLang => StrOrNull("іноземна");
        private string StrOrNull(string str)
        {
            var d = Map.FirstOrDefault(x => x.Key.Contains(str));
            return d.Equals(new KeyValuePair<string, string>()) ? null :
                (d.Value ?? d.Key.Split(' ').Last());
        }
        private Dictionary<string, string> Map => Detail.Split('\n', StringSplitOptions.RemoveEmptyEntries).
            Select(x => x.Split(':')).
            Select(x => new KeyValuePair<string, string>(x[0], x.Length>1?x[1]:null)).Distinct()
            .ToDictionary(x => x.Key.ToLower(), y => y.Value);
    }
}