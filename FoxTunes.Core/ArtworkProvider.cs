using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class ArtworkProvider : StandardComponent, IArtworkProvider
    {
        const int MAX_LENGTH = 1024000;

        public static IDictionary<ArtworkType, string[]> Names = GetNames();

        private static IDictionary<ArtworkType, string[]> GetNames()
        {
            return new Dictionary<ArtworkType, string[]>()
            {
                { ArtworkType.FrontCover, new [] { "front", "cover", "folder" } },
                { ArtworkType.BackCover, new [] { "back" } }
            };
        }

        public async Task<MetaDataItem> Find(string path, ArtworkType type)
        {
            var names = default(string[]);
            if (!Names.TryGetValue(type, out names))
            {
                throw new NotImplementedException();
            }
            if (!string.IsNullOrEmpty(Path.GetPathRoot(path)))
            {
                var exception = default(Exception);
                try
                {
                    var directoryName = Path.GetDirectoryName(path);
                    foreach (var name in names)
                    {
                        foreach (var fileName in FileSystemHelper.EnumerateFiles(directoryName, string.Format("{0}.*", name)))
                        {
                            var info = new FileInfo(fileName);
                            if (info.Length <= MAX_LENGTH)
                            {
                                return new MetaDataItem(Enum.GetName(typeof(ArtworkType), type), MetaDataItemType.Image)
                                {
                                    Value = fileName
                                };
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                }
                if (exception != null)
                {
                    await this.OnError(exception);
                }
            }
            return default(MetaDataItem);
        }

        public Task<MetaDataItem> Find(PlaylistItem playlistItem, ArtworkType type)
        {
            var result = playlistItem.MetaDatas.FirstOrDefault(
                 metaDataItem =>
                     metaDataItem.Type == MetaDataItemType.Image &&
                     string.Equals(metaDataItem.Name, Enum.GetName(typeof(ArtworkType), type), StringComparison.OrdinalIgnoreCase) &&
                     File.Exists(metaDataItem.Value)
             );
#if NET40
            return TaskEx.FromResult(result);
#else
            return Task.FromResult(result);
#endif
        }
    }
}
