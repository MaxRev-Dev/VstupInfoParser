using MaxRev.Servers.API;
using MaxRev.Servers.Core.Route;
using MaxRev.Servers.Interfaces;
using MaxRev.Utils.Methods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VstupInfoParser.Extensions;
using VstupInfoParser.ModelsJSON;
using VstupInfoParser.Parsers;

namespace VstupInfoParser
{
    [RouteBase("api")]
    internal class Api : CoreApi
    {
        private CoreParser CurrentParser => Services.GetRequiredService<CoreParser>();


        [Route("regions")]
        public IResponseInfo GetMainTable()
        {
            return Ok(CurrentParser.RegionTable.Serialize());
        }
        [Route("region/{year}/{name}")]
        public async Task<IResponseInfo> GetMainTable(int year, string name)
        {
            var reg = await GetRegion(year, name).ConfigureAwait(false);

            return Ok(reg.Institutes.Distinct().Serialize());
        }
        [Route("instances/{year}/{region}/{namePart}")]
        public async Task<IResponseInfo> GetMainTable(int year, string region, string namePart)
        {
            var reg = await GetRegion(year, region).ConfigureAwait(false);
            var obj = reg.Institutes.Where(x => x.Name.ToLower(MainApp.DefaultCultureInfo).Contains(
                           Uri.UnescapeDataString(namePart).ToLower(MainApp.DefaultCultureInfo))).Distinct();
            return GetResponse(obj, typeof(InstituteMap));
        }

        [Route("instance/{year}/{region}/{namePart}/{type}")]
        public async Task<IResponseInfo> GetForSpecialty
            (int year, string region, string namePart, string type)
        {
            var obj = await GetForSpecialtyQuery(year, region, namePart, type).ConfigureAwait(false);
            return GetResponse(obj, typeof(SpecialtyMap));
        }

        [Route("instance/{year}/{region}/{namePart}/{type}/{degree}")]
        public async Task<IResponseInfo> GetForSpecialtyType
            (int year, string region, string namePart, string type, string degree)
        {
            var pDegree = (Institute.Degree)Enum.Parse(typeof(Institute.Degree), degree);
            var obj = (await GetForSpecialtyQuery(year, region, namePart, type).ConfigureAwait(false))
                .Where(x => x.Degree == pDegree);
            if (Info.Query.HasKey("faculty"))
            {
                obj = obj.Where(x => x.Faculty != null &&
                x.Faculty.Contains(Info.Query["faculty"], StringComparison.InvariantCultureIgnoreCase));
            }

            var final = obj.Select(x =>
            {
                x.FetchAsync().Wait();
                return x;
            });


            if (Info.Query.HasKey("to_files"))
            {
                List<IEnumerable<Student>> list = new List<IEnumerable<Student>>();
                List<string> names = new List<string>();
                foreach (var i in final)
                { 
                    names.Add(i.Name + '_' + year + '_' + i.GlobalId);
                    list.Add(i.Students);
                }
                return AsFileResponse(list, names, typeof(StudentsMap),
                    string.Join('_', Uri.UnescapeDataString(region), Uri.UnescapeDataString(namePart), year, type, degree));
            }
            return GetResponse(final, typeof(SpecialtyMap));
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
            var obj = await GetForSpecialtyQuery(year, region, namePart, type).ConfigureAwait(false);
            var pDegree = (Institute.Degree)Enum.Parse(typeof(Institute.Degree), degree);
            var q = obj
                .Where(x => x.Degree == pDegree)
                .FirstOrDefault(x => x.GlobalId == gId);
            if (q != default)
            {
                await q.FetchAsync();
                return GetResponse(q.Students, typeof(SpecialtyMap));
            }

            return NotFound();
        }


        private IResponseInfo GetResponse<T>(IEnumerable<T> obj, Type type = null)
        {
            if (Info.Query.HasKey("csv"))
            {
                var text = obj.ToCsv(type);
                var mem = new MemoryStream();
                using (var wr = new StreamWriter(mem, Encoding.UTF8, 4096, true))
                {
                    wr.WriteAsync(text).Wait();
                }

                return SendFile(mem, WebUtility.UrlDecode(Info.Action).Slugify(), "text/csv");
            }
            return Ok(obj.Serialize());
        }

        private async Task<Region> GetRegion(int year, string name)
        {
            var reg = CurrentParser.GetForRegion(year, Uri.UnescapeDataString(name));
            await reg.FetchAsync().ConfigureAwait(false);
            return reg;
        }

        private async Task<IEnumerable<Specialty>> GetForSpecialtyQuery
            (int year, string region, string namePart, string type)
        {
            var reg = await GetRegion(year, region).ConfigureAwait(false);
            var p = Uri.UnescapeDataString(namePart).ToLower(MainApp.DefaultCultureInfo);
            var obj = reg.Institutes.Where(x =>
                x.Name.ToLower(MainApp.DefaultCultureInfo).Contains(p)).Distinct().FirstOrDefault();
            if (obj == default)
            {
                return default;
            }

            await obj.FetchAsync().ConfigureAwait(false);
            var pType = (Institute.StudyType)Enum.Parse(typeof(Institute.StudyType), type);

            return obj.Specialties[pType];
        }
    }
}