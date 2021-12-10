using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface ILibraryHierarchyBrowser : IStandardComponent
    {
        string Filter { get; set; }

        event EventHandler FilterChanged;

        LibraryHierarchyBrowserState State { get; }

        event EventHandler StateChanged;

        LibraryHierarchy[] GetHierarchies();

        LibraryHierarchyNode GetNode(LibraryHierarchy libraryHierarchy, LibraryHierarchyNode libraryHierarchyNode);

        LibraryHierarchyNode[] GetNodes(LibraryHierarchy libraryHierarchy);

        LibraryHierarchyNode[] GetNodes(LibraryHierarchy libraryHierarchy, string filter);

        LibraryHierarchyNode[] GetNodes(LibraryHierarchyNode libraryHierarchyNode);

        LibraryHierarchyNode[] GetNodes(LibraryHierarchyNode libraryHierarchyNode, string filter);

        IEnumerable<LibraryItem> GetItems(LibraryHierarchyNode libraryHierarchyNode, bool loadMetaData);
    }

    public enum LibraryHierarchyBrowserState : byte
    {
        None,
        Loading
    }
}
