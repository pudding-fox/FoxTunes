using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataSource : IBaseComponent
    {
        Task<IEnumerable<MetaDataItem>> GetMetaData(string fileName);

        Task SetMetaData(string fileName, IEnumerable<MetaDataItem> metaDataItems, Func<MetaDataItem, bool> predicate);
    }

    [Flags]
    public enum ArtworkType : byte
    {
        None = 0,
        FrontCover = 1,
        BackCover = 2,
        Unknown = 255
    }
}
