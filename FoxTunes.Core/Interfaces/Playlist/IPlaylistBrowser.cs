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

        PlaylistItem GetItemById(Playlist playlist, int id);

        PlaylistItem GetItemBySequence(Playlist playlist, int sequence);

        PlaylistItem GetFirstItem(Playlist playlist);

        PlaylistItem GetLastItem(Playlist playlist);

        PlaylistItem GetNextItem(Playlist playlist);

        PlaylistItem GetNextItem(PlaylistItem playlistItem);

        PlaylistItem GetPreviousItem(Playlist playlist);

        PlaylistItem GetPreviousItem(PlaylistItem playlistItem);

        int GetInsertIndex(Playlist playlist);

        Task Enqueue(Playlist playlist, PlaylistItem playlistItem, PlaylistQueueFlags flags);

        Task<int> GetQueuePosition(Playlist playlist, PlaylistItem playlistItem);
    }

    public enum PlaylistBrowserState : byte
    {
        None,
        Loading
    }
}
