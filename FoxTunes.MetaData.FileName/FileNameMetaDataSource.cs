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

        public IArtworkProvider ArtworkProvider { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ArtworkProvider = core.Components.ArtworkProvider;
            base.InitializeComponent(core);
        }

        public async Task<IEnumerable<MetaDataItem>> GetMetaData(string fileName)
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
            var metaDataItem = await this.ArtworkProvider.Find(fileName, ArtworkType.FrontCover);
            if (metaDataItem != null)
            {
                result.Add(metaDataItem);
            }
            return result;
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
