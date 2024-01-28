using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface ILibraryHierarchyBrowser : IStandardComponent
    {
        IEnumerable<LibraryHierarchyNode> GetRootNodes(LibraryHierarchy libraryHierarchy);

        IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyNode libraryHierarchyNode);

        IEnumerable<MetaDataItem> GetMetaData(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType metaDataItemType);
    }
}
