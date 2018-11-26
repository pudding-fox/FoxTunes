using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class PlayNextItemBehaviour : StandardBehaviour
    {
        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.BackgroundTaskRunner = core.Components.BackgroundTaskRunner;
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
            Logger.Write(this, LogLevel.Debug, "Stream was stopped likely due to reaching the end, playing next item.");
            this.BackgroundTaskRunner.Run(() => this.PlaylistManager.Next());
        }
    }
}
