using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IBassPluginLoader : IBaseComponent, IDisposable
    {
        IEnumerable<string> Extensions { get; }

        bool IsSupported(string extension);

        bool IsLoaded { get; }

        event EventHandler IsLoadedChanged;
    }
}
