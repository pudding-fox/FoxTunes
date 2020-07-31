using FoxTunes.Interfaces;
using System;
using System.IO;

namespace FoxTunes
{
    public static class WaveFormCache
    {
        private static readonly string PREFIX = typeof(WaveFormCache).Name;

        public static bool TryLoad(IOutputStream stream, int resolution, out WaveFormGenerator.WaveFormGeneratorData data)
        {
            var id = GetId(stream, resolution);
            var fileName = default(string);
            if (FileMetaDataStore.Exists(PREFIX, id, out fileName))
            {
                data = Load(fileName);
                return data != null;
            }
            data = null;
            return false;
        }

        private static WaveFormGenerator.WaveFormGeneratorData Load(string fileName)
        {
            try
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return Serializer.Instance.Read(stream) as WaveFormGenerator.WaveFormGeneratorData;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void Save(IOutputStream stream, WaveFormGenerator.WaveFormGeneratorData data)
        {
            var id = GetId(stream, data.Resolution);
            Save(id, data);
        }

        private static void Save(string id, WaveFormGenerator.WaveFormGeneratorData data)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Instance.Write(stream, data);
                stream.Seek(0, SeekOrigin.Begin);
                FileMetaDataStore.Write(PREFIX, id, stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        private static string GetId(IOutputStream stream, int resolution)
        {
            var hashCode = default(int);
            unchecked
            {
                hashCode = (hashCode * 29) + stream.FileName.GetHashCode();
                hashCode = (hashCode * 29) + resolution.GetHashCode();
            }
            return Math.Abs(hashCode).ToString();
        }
    }
}
