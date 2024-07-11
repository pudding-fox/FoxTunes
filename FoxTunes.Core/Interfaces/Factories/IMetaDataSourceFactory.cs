using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataSourceFactory : IStandardComponent
    {
        IEnumerable<KeyValuePair<string, MetaDataItemType>> Supported { get; }

        IMetaDataSource Create();
    }
}
