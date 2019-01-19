using CsvHelper.Configuration;

namespace VstupInfoParser.ModelsJSON
{
    public sealed class StudentsMap : ClassMap<Student>
    {
        public StudentsMap()
        {
            AutoMap();
        }
    }
}