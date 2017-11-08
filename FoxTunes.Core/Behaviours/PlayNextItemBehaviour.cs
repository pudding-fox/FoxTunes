using FoxTunes.Interfaces;
using System;

namespace FoxTunes.Behaviours
{
    public class PlayNextItemBehaviour : StandardBehaviour
    {
        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public PlaylistItem NextPlaylistItem { get; private set; }

        public override void InitializeComponent(ICore core)
        {
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
            this.NextPlaylistItem = this.PlaylistManager.GetNext();
            this.PlaybackManager.CurrentStream.Stopped += this.CurrentStream_Stopped;
        }

        protected virtual void CurrentStream_Stopped(object sender, StoppedEventArgs e)
        {
            if (e.Manual || this.NextPlaylistItem == null)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Stream was stopped likely due to reaching the end, playing next item.");
            this.PlaylistManager.Play(this.NextPlaylistItem);
        }
    }
}
