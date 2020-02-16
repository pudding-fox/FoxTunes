using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataBrowser : IStandardComponent
    {
        IEnumerable<MetaDataItem> GetMetaDatas(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType);
    }
}
