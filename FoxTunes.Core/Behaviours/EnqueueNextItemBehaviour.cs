using FoxTunes.Interfaces;
using System;
using System.Linq;

namespace FoxTunes.Behaviours
{
    public class EnqueueNextItemBehaviour : StandardBehaviour
    {
        const int ENQUEUE_COUNT = 3;

        public IPlaylist Playlist { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IOutputStreamQueue OutputStreamQueue { get; private set; }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Playlist = core.Components.Playlist;
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
            this.BackgroundTaskRunner.Run(async () =>
            {
                var sequence = this.PlaybackManager.CurrentStream.PlaylistItem.Sequence;
                var query =
                    from playlistItem in this.Playlist.Query
                    orderby playlistItem.Sequence
                    where playlistItem.Sequence > sequence
                    select playlistItem;
                Logger.Write(this, LogLevel.Debug, "Preemptively buffering {0} items.", ENQUEUE_COUNT);
                foreach (var playlistItem in query.Take(ENQUEUE_COUNT))
                {
                    if (this.OutputStreamQueue.IsQueued(playlistItem))
                    {
                        Logger.Write(this, LogLevel.Debug, "Playlist item already buffered: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                        continue;
                    }
                    Logger.Write(this, LogLevel.Debug, "Preemptively buffering playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
                    await this.PlaybackManager.Load(playlistItem, false);
                }
            });
        }
    }
}
