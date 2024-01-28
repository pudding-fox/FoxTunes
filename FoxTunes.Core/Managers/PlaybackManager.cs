using FoxTunes.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes.Managers
{
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
                await this.Unload();
            }
        }

        protected virtual async void OutputStreamQueueDequeued(object sender, OutputStreamQueueEventArgs e)
        {
            using (e.Defer())
            {
                Logger.Write(this, LogLevel.Debug, "Output stream is about to change, pre-empting the next stream: {0} => {1}", e.OutputStream.Id, e.OutputStream.FileName);
                if (!await this.Output.Preempt(e.OutputStream))
                {
                    Logger.Write(this, LogLevel.Debug, "Preempt failed for stream: {0} => {1}", e.OutputStream.Id, e.OutputStream.FileName);
                }
                Logger.Write(this, LogLevel.Debug, "Output stream de-queued, loading it: {0} => {1}", e.OutputStream.Id, e.OutputStream.FileName);
                var exception = default(Exception);
                try
                {
                    await this.SetCurrentStream(e.OutputStream);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                if (exception != null)
                {
                    await this.OnError(exception);
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
                await this.Unload(currentStream);
            }
            else
            {
                this.OnCurrentStreamChanging();
                this._CurrentStream = stream;
            }
            if (stream != null)
            {
                Logger.Write(this, LogLevel.Debug, "Playing stream: {0} => {1}", stream.Id, stream.FileName);
                await stream.Play();
            }
            await this.OnCurrentStreamChanged();
        }


        protected virtual void OnCurrentStreamChanging()
        {
            if (this.CurrentStream != null)
            {
                this.CurrentStream.IsPlayingChanged -= this.IsPlayingChanged;
                this.CurrentStream.IsPausedChanged -= this.IsPausedChanged;
                this.CurrentStream.IsStoppedChanged -= this.IsStoppedChanged;
                this.CurrentStream.Stopping -= this.Stopping;
                this.CurrentStream.Stopped -= this.Stopped;
            }
        }

        protected virtual async Task OnCurrentStreamChanged()
        {
            if (this.CurrentStream != null)
            {
                this.CurrentStream.IsPlayingChanged += this.IsPlayingChanged;
                this.CurrentStream.IsPausedChanged += this.IsPausedChanged;
                this.CurrentStream.IsStoppedChanged += this.IsStoppedChanged;
                this.CurrentStream.Stopping += this.Stopping;
                this.CurrentStream.Stopped += this.Stopped;
            }
            if (this.CurrentStreamChanged != null)
            {
                var e = new AsyncEventArgs();
                this.CurrentStreamChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("CurrentStream");
        }

        public event AsyncEventHandler CurrentStreamChanged = delegate { };

        public event AsyncEventHandler IsPlayingChanged = delegate { };

        public event AsyncEventHandler IsPausedChanged = delegate { };

        public event AsyncEventHandler IsStoppedChanged = delegate { };

        public event AsyncEventHandler Stopping = delegate { };

        public event StoppedEventHandler Stopped = delegate { };

        public async Task Load(PlaylistItem playlistItem, bool immediate)
        {
            using (var task = new LoadOutputStreamTask(playlistItem, immediate))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task);
                await task.Run();
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
                await this.OnBackgroundTask(task);
                await task.Run();
            }
        }

        public async Task Stop()
        {
            await this.SetCurrentStream(null);
            await this.Output.Shutdown();
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

        public event BackgroundTaskEventHandler BackgroundTask = delegate { };

        protected override async void OnDisposing()
        {
            await this.Unload();
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
