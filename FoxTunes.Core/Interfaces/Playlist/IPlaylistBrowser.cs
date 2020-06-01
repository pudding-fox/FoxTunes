using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistBrowser : IStandardComponent
    {
        PlaylistBrowserState State { get; }

        event EventHandler StateChanged;

        Playlist[] GetPlaylists();

        Playlist GetPlaylist(PlaylistItem playlistItem);

        PlaylistItem[] GetItems(Playlist playlist);

        Task<PlaylistItem> GetItem(Playlist playlist, int sequence);

        Task<PlaylistItem> GetItem(Playlist playlist, string fileName);

        Task<PlaylistItem> GetNextItem(Playlist playlist);

        Task<PlaylistItem> GetNextItem(PlaylistItem playlistItem);

        Task<PlaylistItem> GetPreviousItem(Playlist playlist);

        Task<PlaylistItem> GetPreviousItem(PlaylistItem playlistItem);

        Task<int> GetInsertIndex(Playlist playlist);
    }

    public enum PlaylistBrowserState : byte
    {
        None,
        Loading
    }
}
