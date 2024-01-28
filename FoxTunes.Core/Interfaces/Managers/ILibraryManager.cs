using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface ILibraryManager : IStandardManager, IBackgroundTaskSource
    {
        void Add(IEnumerable<string> paths);

        event EventHandler Updated;

        void Clear();
    }
}
