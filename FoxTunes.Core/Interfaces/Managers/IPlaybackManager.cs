using System;

namespace FoxTunes.Interfaces
{
    public interface IPlaybackManager : IStandardManager
    {
        bool IsSupported(string fileName);

        IOutputStream CurrentStream { get; }

        event EventHandler CurrentStreamChanging;

        event EventHandler CurrentStreamChanged;

        void Load(string fileName);

        void Unload();
    }
}
