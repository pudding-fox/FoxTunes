using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistBrowser : IStandardComponent
    {
        PlaylistBrowserState State { get; }

        event EventHandler StateChanged;

        PlaylistItem[] GetItems();

        Task<PlaylistItem> Get(int sequence);

        Task<PlaylistItem> Get(string fileName);

        Task<PlaylistItem> GetNext(bool navigate);

        Task<PlaylistItem> GetNext(PlaylistItem playlistItem);

        Task<PlaylistItem> GetPrevious(bool navigate);

        Task<PlaylistItem> GetPrevious(PlaylistItem playlistItem);

        Task<int> GetInsertIndex();
    }

    public enum PlaylistBrowserState : byte
    {
        None,
        Loading
    }
}
