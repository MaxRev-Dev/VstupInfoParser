using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VstupInfoParser.ModelsJSON
{
    public partial class Institute
    {
        [JsonProperty("instance_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public InstanceType Type { get; set; }
        [JsonIgnore]
        public Dictionary<StudyType, List<Specialty>> Specialties { get; } = new Dictionary<StudyType, List<Specialty>>();
    }
}