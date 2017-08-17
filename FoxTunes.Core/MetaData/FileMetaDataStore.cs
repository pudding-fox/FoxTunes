using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public static class FileMetaDataStore
    {
        private static readonly string DataStoreDirectoryName = Path.Combine(
            Path.GetDirectoryName(typeof(FileMetaDataStore).Assembly.Location),
            "DataStore"
        );

        public static bool Exists(string id, out string fileName)
        {
            fileName = GetFileName(id);
            return File.Exists(fileName);
        }

        public static Task<string> Write(string id, byte[] data)
        {
            var fileName = GetFileName(id);
            LogManager.Logger.Write(typeof(FileMetaDataStore), LogLevel.Trace, "Writing data: {0} => {1}", id, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            File.WriteAllBytes(fileName, data);
            return Task.FromResult(fileName);
        }

        public static async Task<string> Write(string id, Stream stream)
        {
            var fileName = GetFileName(id);
            LogManager.Logger.Write(typeof(FileMetaDataStore), LogLevel.Trace, "Writing data: {0} => {1}", id, fileName);
            using (var file = File.OpenWrite(fileName))
            {
                await stream.CopyToAsync(file);
            }
            return fileName;
        }

        private static IEnumerable<string> GetSegments(string id)
        {
            return BitConverter.GetBytes(id.GetHashCode()).Select(value => Convert.ToString(value));
        }

        public static string GetFileName(string id)
        {
            return Path.Combine(DataStoreDirectoryName, string.Concat(Path.Combine(GetSegments(id).ToArray()), ".bin"));
        }
    }
}
