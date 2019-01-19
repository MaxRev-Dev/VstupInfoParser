using CsvHelper.Configuration;

namespace VstupInfoParser.ModelsJSON
{
    public sealed class SpecialtyMap : ClassMap<Specialty>
    {
        public SpecialtyMap()
        {
            Map(x => x.GlobalId);
            Map(x => x.Degree);
            Map(x => x.Type);
            Map(x => x.Name);
            Map(x => x.Url);
            Map(x => x.Faculty);
            
            Map(x => x.OnProcessError).Ignore();
            Map(x => x.Fetched).Ignore();
        }
    }
}