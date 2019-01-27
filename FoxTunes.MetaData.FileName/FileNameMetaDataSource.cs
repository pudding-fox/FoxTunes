using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class FileNameMetaDataSource : BaseComponent, IMetaDataSource
    {
        public static IDictionary<string, string> Lookup = GetLookup();

        private static IDictionary<string, string> GetLookup()
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            lookup.Add("artist", CommonMetaData.FirstAlbumArtist);
            lookup.Add("performer", CommonMetaData.FirstPerformer);
            lookup.Add("genre", CommonMetaData.FirstGenre);
            return lookup;
        }

        public FileNameMetaDataSource(IEnumerable<IFileNameMetaDataExtractor> extractors)
        {
            this.Extractors = extractors;
        }

        public IEnumerable<IFileNameMetaDataExtractor> Extractors { get; private set; }

        public Task<IEnumerable<MetaDataItem>> GetMetaData(string fileName)
        {
            var result = new List<MetaDataItem>();
            var metaData = default(IDictionary<string, string>);
            foreach (var extractor in this.Extractors)
            {
                if (!extractor.Extract(fileName, out metaData))
                {
                    continue;
                }
                foreach (var key in metaData.Keys)
                {
                    var name = default(string);
                    if (!CommonMetaData.Lookup.TryGetValue(key, out name) && !Lookup.TryGetValue(key, out name))
                    {
                        name = key;
                    }
                    result.Add(this.GetMetaData(name, metaData[key]));
                }
                break;
            }
#if NET40
            return TaskEx.FromResult<IEnumerable<MetaDataItem>>(result);
#else
            return Task.FromResult<IEnumerable<MetaDataItem>>(result);
#endif
        }

        protected virtual MetaDataItem GetMetaData(string name, string value)
        {
            var numeric = default(int);
            var result = new MetaDataItem(name, MetaDataItemType.Tag);
            if (int.TryParse(value, out numeric))
            {
                result.NumericValue = numeric;
            }
            else
            {
                result.TextValue = value;
            }
            return result;
        }
    }
}
