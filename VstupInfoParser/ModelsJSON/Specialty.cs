using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VstupInfoParser.ModelsJSON
{
    public partial class Specialty
    {
        [JsonProperty("gID")]
        public int GlobalId { get; private set; }
        [JsonProperty("students")]
        public List<Student> Students { get; } = new List<Student>();
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
}