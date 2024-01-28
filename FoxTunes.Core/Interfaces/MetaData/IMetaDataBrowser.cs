using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataBrowser : IStandardComponent
    {
        MetaDataItem[] GetMetaDatas(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType? metaDataItemType, string metaDataItemName);

        Task<MetaDataItem[]> GetMetaDatasAsync(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType? metaDataItemType, string metaDataItemName);

        MetaDataItem[] GetMetaDatas(IEnumerable<PlaylistItem> playlistItems, MetaDataItemType? metaDataItemType, string metaDataItemName);

        Task<MetaDataItem[]> GetMetaDatasAsync(IEnumerable<PlaylistItem> playlistItems, MetaDataItemType? metaDataItemType, string metaDataItemName);
    }
}
