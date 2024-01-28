using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IPlaybackManager : IStandardManager, IBackgroundTaskSource, IDisposable
    {
        bool IsSupported(string fileName);

        IOutputStream CurrentStream { get; }

        event AsyncEventHandler CurrentStreamChanged;

        event AsyncEventHandler IsPlayingChanged;

        event AsyncEventHandler IsPausedChanged;

        event AsyncEventHandler IsStoppedChanged;

        event AsyncEventHandler Stopping;

        event StoppedEventHandler Stopped;

        Task Load(PlaylistItem playlistItem, bool immediate);

        Task Stop();
    }
}
