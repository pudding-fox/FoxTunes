using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface ILibraryManager : IStandardManager, IBackgroundTaskSource
    {
        LibraryHierarchy SelectedHierarchy { get; set; }

        event EventHandler SelectedHierarchyChanged;

        LibraryHierarchyNode SelectedNode { get; set; }

        event EventHandler SelectedNodeChanged;

        bool CanNavigate { get; }

        Task Add(IEnumerable<string> paths);

        Task Rescan();

        Task Clear();
    }
}
