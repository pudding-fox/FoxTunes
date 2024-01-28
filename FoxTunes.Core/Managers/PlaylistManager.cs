using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace FoxTunes.Managers
{
    public class PlaylistManager : StandardManager, IPlaylistManager
    {
        public IPlaylist Playlist { get; private set; }

        public IPlaylistItemFactory PlaylistItemFactory { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Playlist = core.Components.Playlist;
            this.PlaylistItemFactory = core.Factories.PlaylistItem;
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        public void AddDirectory(string directoryName)
        {
            foreach (var fileName in Directory.GetFiles(directoryName))
            {
                this.AddFile(fileName);
            }
        }

        public void AddFile(string fileName)
        {
            if (!this.PlaybackManager.IsSupported(fileName))
            {
                return;
            }
            this.Playlist.Items.Add(this.PlaylistItemFactory.Create(fileName));
        }

        public void Next()
        {
            var index = default(int);
            if (this.Playlist.SelectedItem == null)
            {
                index = 0;
            }
            else
            {
                index = this.Playlist.Items.IndexOf(this.Playlist.SelectedItem) + 1;
            }
            if (index >= this.Playlist.Items.Count)
            {
                index = 0;
            }
            this.Playlist.SelectedItem = this.Playlist.Items[index];
        }

        public void Previous()
        {
            var index = default(int);
            if (this.Playlist.SelectedItem == null)
            {
                index = 0;
            }
            else
            {
                index = this.Playlist.Items.IndexOf(this.Playlist.SelectedItem) - 1;
            }
            if (index < 0)
            {
                index = this.Playlist.Items.Count - 1;
            }
            this.Playlist.SelectedItem = this.Playlist.Items[index];
        }

        public IEnumerable<IPlaylistItem> Items
        {
            get
            {
                return this.Playlist.Items;
            }
        }
    }
}
