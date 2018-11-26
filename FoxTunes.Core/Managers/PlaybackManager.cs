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
            await this.ForegroundTaskRunner.RunAsync(() =>
            {
                this._CurrentStream = stream;
                this.OnCurrentStreamChanged();
            });
        }

        public event EventHandler CurrentStreamChanging = delegate { };

        protected virtual void OnCurrentStreamChanged()
        {
            if (this.CurrentStreamChanged != null)
            {
                this.CurrentStreamChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("CurrentStream");
        }

        public event EventHandler CurrentStreamChanged = delegate { };

        public Task Load(PlaylistItem playlistItem, bool immediate)
        {
            var task = new LoadOutputStreamTask(playlistItem, immediate);
            task.InitializeComponent(this.Core);
            this.OnBackgroundTask(task);
            return task.Run();
        }

        public Task Unload()
        {
            if (this.CurrentStream == null)
            {
                return Task.CompletedTask;
            }
            return this.Unload(this.CurrentStream);
        }

        public Task Unload(IOutputStream outputStream)
        {
            var task = new UnloadOutputStreamTask(outputStream);
            task.InitializeComponent(this.Core);
            this.OnBackgroundTask(task);
            return task.Run();
        }

        public Task StopStream()
        {
            if (this.CurrentStream == null)
            {
                return Task.CompletedTask;
            }
            var task = new StopOutputStreamTask();
            task.InitializeComponent(this.Core);
            this.OnBackgroundTask(task);
            return task.Run();
        }

        public async Task StopOutput()
        {
            await this.StopStream();
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
            this.Unload();
        }

        ~PlaybackManager()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
