using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataBrowser : IStandardComponent
    {
        IEnumerable<string> GetMetaDataNames();

        IEnumerable<MetaDataItem> GetMetaDatas(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType);
    }
}
