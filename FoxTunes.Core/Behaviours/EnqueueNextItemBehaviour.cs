using FoxTunes.Interfaces;
using System;
using System.Linq;

namespace FoxTunes.Behaviours
{
    public class EnqueueNextItemBehaviour : StandardBehaviour
    {
        const int ENQUEUE_COUNT = 3;

        public IDataManager DataManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IOutputStreamQueue OutputStreamQueue { get; private set; }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.DataManager = core.Managers.Data;
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
            var sequence = this.PlaybackManager.CurrentStream.PlaylistItem.Sequence;
            var query =
                from playlistItem in this.DataManager.ReadContext.Queries.PlaylistItem
                orderby playlistItem.Sequence
                where playlistItem.Sequence > sequence
                select playlistItem;
            var playlistItems = query.Take(ENQUEUE_COUNT).ToArray();
            this.BackgroundTaskRunner.Run(async () =>
            {
                Logger.Write(this, LogLevel.Debug, "Preemptively buffering {0} items.", ENQUEUE_COUNT);
                foreach (var playlistItem in playlistItems)
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
