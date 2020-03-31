using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface ILibraryManager : IStandardManager, IBackgroundTaskSource, IReportSource
    {
        LibraryManagerState State { get; }

        LibraryHierarchy SelectedHierarchy { get; set; }

        event EventHandler SelectedHierarchyChanged;

        LibraryHierarchyNode SelectedItem { get; set; }

        event EventHandler SelectedItemChanged;

        Task Add(IEnumerable<string> paths);

        Task Rescan();

        Task Rescan(IEnumerable<string> roots, IEnumerable<LibraryItem> items);

        Task Set(LibraryItemStatus status);

        Task Clear(LibraryItemStatus? status);

        Task SetRating(LibraryHierarchyNode libraryHierarchyNode, byte rating);
    }

    [Flags]
    public enum LibraryManagerState : byte
    {
        None = 0,
        Updating = 1
    }
}
