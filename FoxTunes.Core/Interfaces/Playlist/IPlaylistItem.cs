namespace FoxTunes.Interfaces
{
    public interface IPlaylistItem : IBaseComponent
    {
        IPlaylist Playlist { get; }

        IPlaylistItems Items { get; }

        string FileName { get; }
    }
}
