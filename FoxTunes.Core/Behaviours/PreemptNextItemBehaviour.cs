using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PreemptNextItemBehaviour : StandardBehaviour, IDisposable
    {
        public IOutput Output { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IOutputStreamQueue OutputStreamQueue { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.Ending += this.OnEnding;
            this.OutputStreamQueue = core.Components.OutputStreamQueue;
            base.InitializeComponent(core);
        }

        protected virtual async void OnEnding(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await this.PreemptItems();
            }
        }

        private async Task PreemptItems()
        {
            var playlistItem = await this.PlaylistManager.GetNext(false);
            if (playlistItem == null)
            {
                return;
            }
            var outputStream = this.OutputStreamQueue.Peek(playlistItem);
            if (outputStream == null)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Current stream is about to end, pre-empting the next stream: {0} => {1}", outputStream.Id, outputStream.FileName);
            if (!await this.Output.Preempt(outputStream))
            {
                Logger.Write(this, LogLevel.Debug, "Pre-empt failed for stream: {0} => {1}", outputStream.Id, outputStream.FileName);
            }
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
                this.PlaybackManager.Ending -= this.OnEnding;
            }
        }

        ~PreemptNextItemBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
