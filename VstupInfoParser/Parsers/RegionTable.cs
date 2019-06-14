using System.Collections.Generic;
using VstupInfoParser.ModelsJSON;

namespace VstupInfoParser.Parsers
{
    public class RegionTable
    {
        public Instance this[int index]
        {
            get => Regions[index];
            set => Regions[index] = value;
        }
        public Dictionary<int, Instance> Regions { get; } = new Dictionary<int, Instance>();
    }
}