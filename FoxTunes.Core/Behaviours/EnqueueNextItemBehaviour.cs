using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class EnqueueNextItemBehaviour : StandardBehaviour, IDisposable
    {
        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IOutputStreamQueue OutputStreamQueue { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.PlaybackManager = core.Managers.Playback;
            this.OutputStreamQueue = core.Components.OutputStreamQueue;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            base.InitializeComponent(core);
        }

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(() => this.EnqueueItems());
#else
            var task = Task.Run(() => this.EnqueueItems());
#endif
        }

        private async Task EnqueueItems()
        {
            if (this.PlaybackManager.CurrentStream == null)
            {
                return;
            }
            var playlistItem = await this.PlaylistBrowser.GetNext(false).ConfigureAwait(false);
            if (playlistItem == null)
            {
                return;
            }
            if (this.OutputStreamQueue.IsQueued(playlistItem))
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Preemptively buffering playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
            await this.PlaybackManager.Load(playlistItem, false).ConfigureAwait(false);
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
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
