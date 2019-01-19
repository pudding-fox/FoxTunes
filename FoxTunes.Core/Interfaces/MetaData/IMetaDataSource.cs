using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataSource : IBaseComponent
    {
        Task<IEnumerable<MetaDataItem>> GetMetaData(string fileName);
    }

    [Flags]
    public enum MetaDataCategory : byte
    {
        None = 0,
        Standard = 1,
        Extended = 2,
        First = 4,
        Sort = 8,
        Joined = 16,
        MusicBrainz = 32,
        MultiMedia = 64
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
