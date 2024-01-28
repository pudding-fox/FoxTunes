using FoxTunes.Interfaces;
using System;

namespace FoxTunes.Behaviours
{
    public class PlaySelectedItemBehaviour : StandardBehaviour
    {
        public IPlaylist Playlist { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Playlist = core.Components.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.Playlist.SelectedItemChanging += this.Playlist_SelectedItemChanging;
            this.Playlist.SelectedItemChanged += this.Playlist_SelectedItemChanged;
            base.InitializeComponent(core);
        }

        protected virtual void Playlist_SelectedItemChanging(object sender, EventArgs e)
        {
            if (this.PlaybackManager.CurrentStream == null)
            {
                return;
            }
            this.PlaybackManager.CurrentStream.Stop();
            this.PlaybackManager.Unload();
        }

        protected virtual void Playlist_SelectedItemChanged(object sender, EventArgs e)
        {
            if (this.Playlist.SelectedItem == null)
            {
                return;
            }
            this.PlaybackManager.Load(this.Playlist.SelectedItem.FileName);
            this.PlaybackManager.CurrentStream.Play();
        }
    }
}
