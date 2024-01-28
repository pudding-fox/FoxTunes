using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes.Managers
{
    public class PlaybackManager : StandardManager, IPlaybackManager
    {
        public ICore Core { get; private set; }

        public IOutput Output { get; private set; }

        public IOutputStreamQueue OutputStreamQueue { get; private set; }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output;
            if (this.Output != null)
            {
                this.Output.IsStartedChanged += (sender, e) =>
                {
                    //TODO: Bad awaited Task.
                    this.BackgroundTaskRunner.Run(() => this.Unload());
                };
            }
            this.OutputStreamQueue = core.Components.OutputStreamQueue;
            this.OutputStreamQueue.Dequeued += this.OutputStreamQueueDequeued;
            this.BackgroundTaskRunner = core.Components.BackgroundTaskRunner;
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            base.InitializeComponent(core);
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
                await this.SetCurrentStream(e.OutputStream);
                Logger.Write(this, LogLevel.Debug, "Output stream loaded: {0} => {1}", this.CurrentStream.Id, this.CurrentStream.FileName);
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
            if (this.CurrentStream != null)
            {
                if (object.ReferenceEquals(this.CurrentStream, stream))
                {
                    return;
                }
                Logger.Write(this, LogLevel.Debug, "Unloading current stream: {0} => {1}", this.CurrentStream.Id, this.CurrentStream.FileName);
                await this.Unload(this.CurrentStream);
            }
            this._CurrentStream = stream;
            await this.OnCurrentStreamChanged();
        }

        protected virtual async Task OnCurrentStreamChanged()
        {
            if (this.CurrentStreamChanged != null)
            {
                var e = new AsyncEventArgs();
                this.CurrentStreamChanged(this, e);
                await e.Complete();
            }
            this.OnPropertyChanged("CurrentStream");
        }

        public event AsyncEventHandler CurrentStreamChanged = delegate { };

        public async Task Load(PlaylistItem playlistItem, bool immediate)
        {
            using (var task = new LoadOutputStreamTask(playlistItem, immediate))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run();
            }
        }

        public Task Unload()
        {
            if (this.CurrentStream == null)
            {
                return Task.CompletedTask;
            }
            return this.SetCurrentStream(null);
        }

        public async Task Unload(IOutputStream outputStream)
        {
            using (var task = new UnloadOutputStreamTask(outputStream))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run();
            }
        }

        public async Task Stop()
        {
            await this.SetCurrentStream(null);
            await this.Output.Shutdown();
        }

        protected virtual void OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
                return;
            }
            this.BackgroundTask(this, new BackgroundTaskEventArgs(backgroundTask));
        }

        public event BackgroundTaskEventHandler BackgroundTask = delegate { };

        protected override void OnDisposing()
        {
            //TODO: Bad awaited Task.
            this.Unload();
            base.OnDisposing();
        }
    }
}
