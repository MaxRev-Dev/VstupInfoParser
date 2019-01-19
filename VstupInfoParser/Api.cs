using MaxRev.Servers.API;
using MaxRev.Servers.Core.Route;
using MaxRev.Servers.Interfaces;
using MaxRev.Utils.Methods;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VstupInfoParser.Extensions;
using VstupInfoParser.ModelsJSON;
using VstupInfoParser.Parsers;

namespace VstupInfoParser
{
    [RouteBase("api")]
    internal class Api : CoreApi
    {
        private readonly CultureInfo _ci = new CultureInfo("uk-UA");
        [Route("regions")]
        public IResponseInfo GetMainTable()
        {
            return Ok(CoreParser.Current.RegionTable.Serialize());
        }
        [Route("region/{year}/{name}")]
        public async Task<IResponseInfo> GetMainTable(int year, string name)
        {
            var reg = await GetRegion(year, name);

            return Ok(reg.Institutes.Distinct().Serialize());
        }
        [Route("instances/{year}/{region}/{namePart}")]
        public async Task<IResponseInfo> GetMainTable(int year, string region, string namePart)
        {
            var reg = await GetRegion(year, region);
            var obj = reg.Institutes.Where(x => x.Name.ToLower(_ci).Contains(
                           Uri.UnescapeDataString(namePart).ToLower(_ci))).Distinct();
            return GetResponse(obj, typeof(InstituteMap));
        }

        [Route("instance/{year}/{region}/{namePart}/{type}")]
        public async Task<IResponseInfo> GetForSpecialty
            (int year, string region, string namePart, string type)
        {
            var obj = await GetForSpecialtyQuery(year, region, namePart, type);
            return GetResponse(obj, typeof(SpecialtyMap));
        }

        [Route("instance/{year}/{region}/{namePart}/{type}/{degree}")]
        public async Task<IResponseInfo> GetForSpecialtyType
            (int year, string region, string namePart, string type, string degree)
        {
            var pDegree = (Institute.Degree)Enum.Parse(typeof(Institute.Degree), degree);
            var obj = (await GetForSpecialtyQuery(year, region, namePart, type))
                .Where(x => x.Degree == pDegree);
            if (Info.Query.HasKey("faculty"))
            {
                obj = obj.Where(x => x.Faculty != null &&
                x.Faculty.ToLower().Contains(Info.Query["faculty"]));
            }
            if (Info.Query.HasKey("to_files"))
            {
                List<IEnumerable<Student>> list = new List<IEnumerable<Student>>();
                List<string> names = new List<string>();
                foreach (var i in obj)
                {
                    await i.Fetch();
                    names.Add(i.Name + '_' + year + '_' + i.GlobalId);
                    list.Add(i.Students);
                }
                return AsFileResponse(list, names, typeof(StudentsMap),
                    string.Join('_', Uri.UnescapeDataString(region), Uri.UnescapeDataString(namePart), year, type, degree));
            }
            return GetResponse(obj, typeof(SpecialtyMap));
        }

        private IResponseInfo AsFileResponse<T>(IEnumerable<IEnumerable<T>> obj,
            IEnumerable<string> names, Type type, string archName = null)
        {
            return Ok(
                obj.ToCsvFile(names, Server.DirectoryManager[MainApp.Dirs.TmpCsv],
                type, "/csv/", archName),
                "text/plain");
        }

        [Route("instance/{year}/{region}/{namePart}/{type}/{degree}/{gID}")]
        public async Task<IResponseInfo> GetForGlobalId
            (int year, string region, string namePart, string type, string degree, int gId)
        {
            var obj = await GetForSpecialtyQuery(year, region, namePart, type);
            var pDegree = (Institute.Degree)Enum.Parse(typeof(Institute.Degree), degree);
            var q = obj
                .Where(x => x.Degree == pDegree)
                .FirstOrDefault(x => x.GlobalId == gId);
            if (q != default)
            {
                await q.Fetch();
                return GetResponse(q.Students, typeof(SpecialtyMap));
            }

            return NotFound();
        }


        private IResponseInfo GetResponse<T>(IEnumerable<T> obj, Type type = null)
        {
            var text = obj.ToCsv(type);
            if (Info.Query.HasKey("csv"))
            {
                return Builder.Content(text).ContentType("text/csv").Build();
            }
            else
                return Ok(obj.Serialize());
        }

        private async Task<Region> GetRegion(int year, string name)
        {
            var reg = CoreParser.Current.GetForRegion(year, Uri.UnescapeDataString(name));
            await reg.Fetch();
            return reg;
        }

        private async Task<IEnumerable<Specialty>> GetForSpecialtyQuery
            (int year, string region, string namePart, string type)
        {
            var reg = await GetRegion(year, region);
            var obj = reg.Institutes.Where(x => x.Name.ToLower(_ci).Contains(
                           Uri.UnescapeDataString(namePart).ToLower(_ci))).Distinct().FirstOrDefault();
            if (obj == default) return default;
            await obj.Fetch();
            var pType = (Institute.StudyType)Enum.Parse(typeof(Institute.StudyType), type);

            return obj.Specialties[pType];
        }
    }
}