using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class FileNameMetaDataSource : BaseComponent, IMetaDataSource
    {
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
                    if (!CommonMetaData.Lookup.TryGetValue(key, out name))
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
            return new MetaDataItem(name, MetaDataItemType.Tag)
            {
                Value = value
            };
        }

        public Task SetMetaData(string fileName, IEnumerable<MetaDataItem> metaDataItems)
        {
            throw new NotImplementedException();
        }
    }
}
