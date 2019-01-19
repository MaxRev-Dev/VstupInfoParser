using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VstupInfoParser.ModelsJSON
{
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
        public DateTime Fetched { get; private set; }
        [JsonIgnore]
        protected int Year { get; }
    }
}