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

        LibraryHierarchyNode[] GetNodes(LibraryHierarchy libraryHierarchy);

        LibraryHierarchyNode[] GetNodes(LibraryHierarchyNode libraryHierarchyNode);

        IEnumerable<LibraryItem> GetItems(LibraryHierarchyNode libraryHierarchyNode, bool loadMetaData);
    }

    public enum LibraryHierarchyBrowserState : byte
    {
        None,
        Loading
    }
}
