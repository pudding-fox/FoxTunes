using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class PlaylistItem : BaseComponent, IPlaylistItem
    {
        public PlaylistItem(IPlaylist playlist, IPlaylistItems items, string fileName)
        {
            this.Playlist = playlist;
            this.Items = items;
            this.FileName = fileName;
        }

        public IPlaylist Playlist { get; private set; }

        public IPlaylistItems Items { get; private set; }

        public string FileName { get; private set; }
    }
}
