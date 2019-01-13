using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class EnqueueNextItemBehaviour : StandardBehaviour, IDisposable
    {
        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IOutputStreamQueue OutputStreamQueue { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.OutputStreamQueue = core.Components.OutputStreamQueue;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            base.InitializeComponent(core);
        }

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
            var task = this.EnqueueItems();
        }

        private async Task EnqueueItems()
        {
            if (this.PlaybackManager.CurrentStream == null)
            {
                return;
            }
            var playlistItem = await this.PlaylistManager.GetNext();
            if (playlistItem == null)
            {
                return;
            }
            if (this.OutputStreamQueue.IsQueued(playlistItem))
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Preemptively buffering playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
            await this.PlaybackManager.Load(playlistItem, false);
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
        }

        ~EnqueueNextItemBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
