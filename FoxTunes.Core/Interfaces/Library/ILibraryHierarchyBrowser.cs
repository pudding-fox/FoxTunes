using FoxDb.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface ILibraryHierarchyBrowser : IStandardComponent
    {
        string Filter { get; set; }

        event EventHandler FilterChanged;

        IEnumerable<LibraryHierarchy> GetHierarchies();

        IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchy libraryHierarchy);

        IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyNode libraryHierarchyNode);
    }
}
