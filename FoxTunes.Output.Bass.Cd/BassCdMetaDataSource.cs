using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassCdMetaDataSource : BaseComponent, IMetaDataSource
    {
        public BassCdMetaDataSource(IBassCdMetaDataSourceStrategy strategy)
        {
            this.Strategy = strategy;
        }

        public IBassCdMetaDataSourceStrategy Strategy { get; private set; }

        public Task<IEnumerable<MetaDataItem>> GetMetaData(string fileName)
        {
            var drive = default(int);
            var track = default(int);
            if (!BassCdStreamProvider.ParseUrl(fileName, out drive, out track))
            {
                //TODO: Warn.
#if NET40
                return TaskEx.FromResult(Enumerable.Empty<MetaDataItem>());
#else
                return Task.FromResult(Enumerable.Empty<MetaDataItem>());
#endif
            }
            var metaData = new List<MetaDataItem>();
            metaData.AddRange(this.Strategy.GetMetaDatas(track));
            metaData.AddRange(this.Strategy.GetProperties(track));
#if NET40
            return TaskEx.FromResult<IEnumerable<MetaDataItem>>(metaData);
#else
            return Task.FromResult<IEnumerable<MetaDataItem>>(metaData);
#endif
        }
    }
}
