using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CsvHelper;

namespace VstupInfoParser.Extensions
{

    public static class Extensions
    {
        public static IEnumerable<object> ToCsvFile<T>
            (this IEnumerable<IEnumerable<T>> xrecords,
            IEnumerable<string> xnames, string path,
            Type map = null, string fromWeb = null, string createArchiveName = null)
        {
            List<IEnumerable<T>> records = xrecords.ToList();
            List<string> names = xnames.ToList();
            List<string> res = new List<string>();
            for (int i = 0; i < records.Count; i++)
            {
                var md5 = names[i].Replace(" ", "_") + ".csv";
                var resPath = Path.Combine(path, md5);
                res.Add(resPath);
                using (var streamWriter = new StreamWriter(resPath, false, Encoding.GetEncoding(1251)))
                using (var csvWriter = new CsvWriter(streamWriter))
                {
                    if (map != null)
                        try
                        {
                            csvWriter.Configuration.RegisterClassMap(map);
                        }
                        catch
                        {
                            // ignored
                        }

                    try
                    {
                        csvWriter.WriteRecords(records[i]);
                    }
                    catch
                    {
                        // ignored
                    }
                }
                var f = (fromWeb ?? "") + md5;
                yield return f;
            }
            if (createArchiveName != null)
            {
                using (var fileStream = new FileStream(Path.Combine(path, createArchiveName + ".zip"),
                    FileMode.CreateNew))

                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    foreach (var file in res)
                    {
                        var info = new FileInfo(file);
                        archive.CreateEntryFromFile(file, info.Name);
                    }
                }
            }
        }
        public static string ToCsv<T>(this IEnumerable<T> records,
            Type map = null)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                if (map != null)
                    try
                    {
                        csvWriter.Configuration.RegisterClassMap(map);
                    }
                    catch
                    {
                        // ignored
                    }

                try
                {
                    csvWriter.WriteRecords(records);
                }
                catch
                {
                    // ignored
                }

                streamWriter.Flush();
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }

}