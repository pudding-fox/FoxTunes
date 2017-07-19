using System;

namespace FoxTunes.Interfaces
{
    public interface IPlaybackManager : IStandardManager
    {
        bool IsSupported(string fileName);

        IOutputStream CurrentStream { get; }

        event EventHandler CurrentStreamChanged;

        IOutputStream Load(string fileName);

        void Unload();
    }
}
