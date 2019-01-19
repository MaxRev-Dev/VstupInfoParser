using CsvHelper.Configuration;

namespace VstupInfoParser.ModelsJSON
{
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
}