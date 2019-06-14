using System.Linq;
using System.Threading.Tasks;
using VstupInfoParser.ModelsJSON;

namespace VstupInfoParser.Parsers
{
    // basic usage
    //var region = GetForRegion(2018, "Київ");
    //await region.Fetch();
    //await region.Institutes.First().Fetch();
    //await region.Institutes.First()
    //    .Specialties[Institute.SType.Full].ElementAt(1)
    //    .Fetch();

    internal class CoreParser
    {
        private const string Host = "http://www.vstup.info/";
        public DynamicRegionTable RegionTable { get; private set; }

        public static string FromBase(string url)
        {
            if (url == null)
            {
                return null;
            }

            return Host.Trim('/') + url.Trim('.');
        }
        internal async Task FetchTableAsync()
        {
            var drt = new DynamicRegionTable(Host);
            await drt.FetchAsync().ConfigureAwait(false);
            RegionTable = drt;
        }

        private RegionTable GetForYear(int year)
        {
            if (RegionTable != null && RegionTable.Years.ContainsKey(year))
            {
                return RegionTable.Years[year];
            }
            return default;
        }

        public Region GetForRegion(int year, string regionName)
        {
            if (GetForYear(year) is var table)
            {
                if (table != null)
                {
                    return table.Regions.Values.FirstOrDefault(x => x.Name.Contains(regionName)) as Region;
                }
            }
            return default;
        }
    }
}