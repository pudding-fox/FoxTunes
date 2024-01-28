using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var index = this.Playlist.Set.IndexOf(this.PlaybackManager.CurrentStream.PlaylistItem) + 1;
                for (var a = 0; a < ENQUEUE_COUNT && index < this.Playlist.Set.Count; a++, index++)
                {
                    var playlistItem = this.Playlist.Set[index];
                    if (this.OutputStreamQueue.IsQueued(playlistItem))
                    {
                        continue;
                    }
                    await this.PlaybackManager.Load(playlistItem, false);
                }
            });
        }
    }
}
