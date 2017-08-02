using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IPlaybackManager : IStandardManager, IBackgroundTaskSource, IDisposable
    {
        bool IsSupported(string fileName);

        IOutputStream CurrentStream { get; }

        event EventHandler CurrentStreamChanged;

        Task Load(PlaylistItem playlistItem, bool immediate);

        Task Unload();

        Task Stop();
    }
}
