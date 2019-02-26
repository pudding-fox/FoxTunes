using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface ILibraryManager : IStandardManager, IBackgroundTaskSource
    {
        LibraryManagerState State { get; }

        LibraryHierarchy SelectedHierarchy { get; set; }

        event EventHandler SelectedHierarchyChanged;

        LibraryHierarchyNode SelectedItem { get; set; }

        event EventHandler SelectedItemChanged;

        bool CanNavigate { get; }

        Task Add(IEnumerable<string> paths);

        Task Rescan();

        Task Clear();
    }

    [Flags]
    public enum LibraryManagerState : byte
    {
        None = 0,
        Updating = 1
    }
}
