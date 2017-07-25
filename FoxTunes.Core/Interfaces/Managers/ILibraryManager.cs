using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface ILibraryManager : IStandardManager
    {
        void Add(IEnumerable<string> paths);

        event EventHandler Updated;

        void Clear();
    }
}
