using FoxTunes.Interfaces;
using System;

namespace FoxTunes.Behaviours
{
    public class PlayNextItemBehaviour : StandardBehaviour
    {
        public IPlaylist Playlist { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Playlist = core.Components.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaybackManager.CurrentStreamChanged += this.PlaybackManager_CurrentStreamChanged;
            base.InitializeComponent(core);
        }

        protected virtual void PlaybackManager_CurrentStreamChanged(object sender, EventArgs e)
        {
            if (this.PlaybackManager.CurrentStream == null)
            {
                return;
            }
            this.PlaybackManager.CurrentStream.Stopped += this.CurrentStream_Stopped;
        }

        protected virtual void CurrentStream_Stopped(object sender, StoppedEventArgs e)
        {
            if (e.Manual)
            {
                return;
            }
            this.PlaylistManager.Next();
        }
    }
}
