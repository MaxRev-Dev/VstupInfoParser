using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VstupInfoParser.Models_JSON
{
    public partial class Specialty
    {
        [JsonProperty("gID")]
        public int GlobalID { get; private set; }
        [JsonProperty("students")]
        public List<Students> Students { get; } = new List<Students>();
        [JsonProperty("degree")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Institute.Degree Degree { get; }
        [JsonProperty("time_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Institute.StudyType Type { get; }
        [JsonProperty("info_map")]
        public IEnumerable<KeyValuePair<string, string>> Map { get; }

        [JsonProperty("faculty")]
        public string Faculty => Map?.Where(x => x.Key.ToLower().Contains("факул"))
                    .FirstOrDefault().Value;
    }
    public partial class Students
    {
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
        public bool Quote { get; set; }
        [JsonProperty("origs")]
        public bool Origs { get; set; }
    }
    public partial class Institute
    {
        [JsonProperty("instance_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public InstanceType Type { get; set; }
        [JsonIgnore]
        public Dictionary<StudyType, List<Specialty>> Specialties { get; } = new Dictionary<StudyType, List<Specialty>>();
    }
    public partial class Region
    {
        [JsonIgnore]
        public List<Institute> Institutes { get; } = new List<Institute>();
    }
    public partial class Instance
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonIgnore]
        public Exception OnProcessError { get; internal set; }
        [JsonIgnore]
        public DateTime Fetched { get; private set; } = default;
        [JsonIgnore]
        protected int Year { get; }
    }

    public sealed class SpecialtyMap : ClassMap<Specialty>
    {
        public SpecialtyMap()
        {
            Map(x => x.GlobalID);
            Map(x => x.Degree);
            Map(x => x.Type);
            Map(x => x.Name);
            Map(x => x.Url);
            Map(x => x.Faculty);
            Map(x => x.OnProcessError).Ignore();
            Map(x => x.Fetched).Ignore();
        }
    }
    public sealed class InstituteMap : ClassMap<Institute>
    {
        public InstituteMap()
        {
            Map(x => x.Name);
            Map(x => x.Type);
            Map(x => x.Url);
            Map(x => x.OnProcessError).Ignore();
            Map(x => x.Fetched).Ignore();
        }
    }
    public sealed class StudentsMap : ClassMap<Students>
    {
        public StudentsMap()
        {
            AutoMap();
        }
    }
}