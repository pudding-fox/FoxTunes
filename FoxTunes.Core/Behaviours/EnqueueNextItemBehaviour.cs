using FoxTunes.Interfaces;
using System;

namespace FoxTunes.Behaviours
{
    public class EnqueueNextItemBehaviour : StandardBehaviour
    {
        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IOutputStreamQueue OutputStreamQueue { get; private set; }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.OutputStreamQueue = core.Components.OutputStreamQueue;
            this.PlaybackManager.CurrentStreamChanged += this.PlaybackManager_CurrentStreamChanged;
            this.BackgroundTaskRunner = core.Components.BackgroundTaskRunner;
            base.InitializeComponent(core);
        }

        protected virtual void PlaybackManager_CurrentStreamChanged(object sender, EventArgs e)
        {
            if (this.PlaybackManager.CurrentStream == null)
            {
                return;
            }
            this.EnqueueItems();
        }

        private void EnqueueItems()
        {
            var playlistItem = this.PlaylistManager.GetNext();
            if (playlistItem == null)
            {
                return;
            }
            if (this.OutputStreamQueue.IsQueued(playlistItem))
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Preemptively buffering playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
            this.BackgroundTaskRunner.Run(async () =>
            {
                try
                {
                    await this.PlaybackManager.Load(playlistItem, false);
                }
                catch
                {
                    //Nothing can be done.
                }
            });
        }
    }
}
