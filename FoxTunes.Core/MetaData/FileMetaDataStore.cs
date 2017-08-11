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

        public static Task Write(string id, byte[] data)
        {
            var segments = GetSegments(id);
            var fileName = GetFileName(segments);
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            File.WriteAllBytes(fileName, data);
            return Task.CompletedTask;
        }

        public static async Task Write(string id, Stream stream)
        {
            var segments = GetSegments(id);
            var fileName = GetFileName(segments);
            using (var file = File.OpenWrite(fileName))
            {
                await stream.CopyToAsync(file);
            }
        }

        public static Task<Stream> Read(string id)
        {
            var segments = GetSegments(id);
            var fileName = GetFileName(segments);
            if (!File.Exists(fileName))
            {
                return Task.FromResult(Stream.Null);
            }
            var file = File.OpenRead(fileName);
            return Task.FromResult<Stream>(file);
        }

        private static IEnumerable<string> GetSegments(string id)
        {
            return BitConverter.GetBytes(id.GetHashCode()).Select(value => Convert.ToString(value));
        }

        private static string GetFileName(IEnumerable<string> segments)
        {
            return Path.Combine(DataStoreDirectoryName, string.Concat(Path.Combine(segments.ToArray()), ".bin"));
        }
    }
}
