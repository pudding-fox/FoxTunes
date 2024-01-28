using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IHierarchyManager : IStandardManager, IDatabaseInitializer
    {
        HierarchyManagerState State { get; }

        Task Build(LibraryItemStatus? status);

        Task Clear(LibraryItemStatus? status, bool signal);

        Task Refresh(params string[] names);

        Task Refresh(IEnumerable<IFileData> fileDatas, params string[] names);
    }

    [Flags]
    public enum HierarchyManagerState : byte
    {
        None = 0,
        Updating = 1
    }
}
