using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IHierarchyManager : IStandardManager, IBackgroundTaskSource, IDatabaseInitializer
    {
        HierarchyManagerState State { get; }

        Task Build(LibraryItemStatus? status);

        Task Clear(LibraryItemStatus? status, bool signal);

        Task Refresh(IEnumerable<IFileData> fileDatas);
    }

    [Flags]
    public enum HierarchyManagerState : byte
    {
        None = 0,
        Updating = 1
    }
}
