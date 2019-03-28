using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IHierarchyManager : IStandardManager, IBackgroundTaskSource
    {
        HierarchyManagerState State { get; }

        Task Build(LibraryItemStatus? status);

        Task Clear(LibraryItemStatus? status);
    }

    [Flags]
    public enum HierarchyManagerState : byte
    {
        None = 0,
        Updating = 1
    }
}
