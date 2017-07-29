using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface ILibraryManager : IStandardManager, IBackgroundTaskSource
    {
        void Add(IEnumerable<string> paths);

        void BuildHierarchies();

        event EventHandler Updated;
    }
}
