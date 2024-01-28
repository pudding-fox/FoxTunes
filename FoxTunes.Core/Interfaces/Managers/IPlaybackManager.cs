using System;

namespace FoxTunes.Interfaces
{
    public interface IPlaybackManager : IStandardManager, IBackgroundTaskSource, IDisposable
    {
        bool IsSupported(string fileName);

        IOutputStream CurrentStream { get; set; }

        event EventHandler CurrentStreamChanged;

        void Load(string fileName, Action callBack = null);

        void Unload(Action callBack = null);
    }
}
