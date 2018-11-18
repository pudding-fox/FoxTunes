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
            ComponentScanner.Instance.Location,
            "DataStore"
        );

        public static readonly object SyncRoot = new object();

        public static bool Exists(string id, out string fileName)
        {
            fileName = GetFileName(id);
            return File.Exists(fileName);
        }

        public static async Task<string> Write(string id, byte[] data)
        {
            var fileName = GetFileName(id);
            LogManager.Logger.Write(typeof(FileMetaDataStore), LogLevel.Trace, "Writing data: {0} => {1}", id, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            try
            {
                using (var stream = File.OpenWrite(fileName))
                {
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                LogManager.Logger.Write(typeof(FileMetaDataStore), LogLevel.Error, "Failed to write data: {0} => {1} => {2}", id, fileName, e.Message);
            }
            return fileName;
        }

        public static async Task<string> Write(string id, Stream stream)
        {
            var fileName = GetFileName(id);
            LogManager.Logger.Write(typeof(FileMetaDataStore), LogLevel.Trace, "Writing data: {0} => {1}", id, fileName);
            using (var file = File.OpenWrite(fileName))
            {
                try
                {
                    await stream.CopyToAsync(file);
                }
                catch (Exception e)
                {
                    LogManager.Logger.Write(typeof(FileMetaDataStore), LogLevel.Error, "Failed to write data: {0} => {1} => {2}", id, fileName, e.Message);
                }
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
