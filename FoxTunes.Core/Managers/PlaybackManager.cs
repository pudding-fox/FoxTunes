using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class PlaybackManager : StandardManager, IPlaybackManager
    {
        public ICore Core { get; private set; }

        public IOutput Output { get; private set; }

        public IOutputStreamQueue OutputStreamQueue { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output;
            this.Output.IsStartedChanged += this.OnIsStartedChanged;
            this.OutputStreamQueue = core.Components.OutputStreamQueue;
            this.OutputStreamQueue.Dequeued += this.OutputStreamQueueDequeued;
            base.InitializeComponent(core);
        }

        protected virtual async void OnIsStartedChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await this.Unload().ConfigureAwait(false);
            }
        }

        protected virtual async void OutputStreamQueueDequeued(object sender, OutputStreamQueueEventArgs e)
        {
            using (e.Defer())
            {
                Logger.Write(this, LogLevel.Debug, "Output stream is about to change, pre-empting the next stream: {0} => {1}", e.OutputStream.Id, e.OutputStream.FileName);
                if (!await this.Output.Preempt(e.OutputStream).ConfigureAwait(false))
                {
                    Logger.Write(this, LogLevel.Debug, "Preempt failed for stream: {0} => {1}", e.OutputStream.Id, e.OutputStream.FileName);
                }
                Logger.Write(this, LogLevel.Debug, "Output stream de-queued, loading it: {0} => {1}", e.OutputStream.Id, e.OutputStream.FileName);
                var exception = default(Exception);
                try
                {
                    await this.SetCurrentStream(e.OutputStream).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                if (exception != null)
                {
                    await this.OnError(exception).ConfigureAwait(false);
                }
            }
        }

        public bool IsSupported(string fileName)
        {
            return this.Output.IsSupported(fileName);
        }

        private IOutputStream _CurrentStream { get; set; }

        public IOutputStream CurrentStream
        {
            get
            {
                return this._CurrentStream;
            }
        }

        protected virtual async Task SetCurrentStream(IOutputStream stream)
        {
            var currentStream = this.CurrentStream;
            if (currentStream != null)
            {
                if (object.ReferenceEquals(currentStream, stream))
                {
                    return;
                }
                Logger.Write(this, LogLevel.Debug, "Unloading current stream: {0} => {1}", currentStream.Id, currentStream.FileName);
                this.OnCurrentStreamChanging();
                this._CurrentStream = stream;
                await this.Unload(currentStream).ConfigureAwait(false);
            }
            else
            {
                this.OnCurrentStreamChanging();
                this._CurrentStream = stream;
            }
            if (stream != null)
            {
                Logger.Write(this, LogLevel.Debug, "Playing stream: {0} => {1}", stream.Id, stream.FileName);
                try
                {
                    await stream.Play().ConfigureAwait(false);
                }
                catch
                {
                    await this.Unload(stream).ConfigureAwait(false);
                    throw;
                }
            }
            await this.OnCurrentStreamChanged().ConfigureAwait(false);
        }


        protected virtual void OnCurrentStreamChanging()
        {
            if (this.CurrentStream != null)
            {
                this.CurrentStream.Ending -= this.Ending;
                this.CurrentStream.Ended -= this.Ended;
            }
        }

        protected virtual async Task OnCurrentStreamChanged()
        {
            if (this.CurrentStream != null)
            {
                this.CurrentStream.Ending += this.Ending;
                this.CurrentStream.Ended += this.Ended;
            }
            if (this.CurrentStreamChanged != null)
            {
                var e = new AsyncEventArgs();
                this.CurrentStreamChanged(this, e);
                await e.Complete().ConfigureAwait(false);
            }
            this.OnPropertyChanged("CurrentStream");
        }

        public event AsyncEventHandler CurrentStreamChanged;

        public event AsyncEventHandler Ending;

        public event AsyncEventHandler Ended;

        public async Task Load(PlaylistItem playlistItem, bool immediate)
        {
            using (var task = new LoadOutputStreamTask(playlistItem, immediate))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public Task Unload()
        {
            if (this.CurrentStream == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.SetCurrentStream(null);
        }

        public async Task Unload(IOutputStream outputStream)
        {
            using (var task = new UnloadOutputStreamTask(outputStream))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task Stop()
        {
            await this.SetCurrentStream(null).ConfigureAwait(false);
            await this.Output.Shutdown().ConfigureAwait(false);
        }

        protected virtual Task OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new BackgroundTaskEventArgs(backgroundTask);
            this.BackgroundTask(this, e);
            return e.Complete();
        }

        public event BackgroundTaskEventHandler BackgroundTask;

        protected override async void OnDisposing()
        {
            await this.Unload().ConfigureAwait(false);
            if (this.Output != null)
            {
                this.Output.IsStartedChanged -= this.OnIsStartedChanged;
            }
            if (this.OutputStreamQueue != null)
            {
                this.OutputStreamQueue.Dequeued -= this.OutputStreamQueueDequeued;
            }
            base.OnDisposing();
        }
    }
}
