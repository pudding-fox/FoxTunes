using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface ILibraryHierarchyCache : IStandardComponent
    {
        IEnumerable<LibraryHierarchy> GetHierarchies(Func<IEnumerable<LibraryHierarchy>> factory);

        IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchy libraryHierarchy, string filter, Func<IEnumerable<LibraryHierarchyNode>> factory);

        IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyNode libraryHierarchyNode, string filter, Func<IEnumerable<LibraryHierarchyNode>> factory);
    }
}
