using FoxTunes.Interfaces;
using System;

namespace FoxTunes.Managers
{
    public class PlaybackManager : StandardManager, IPlaybackManager
    {
        public ICore Core { get; private set; }

        public IOutput Output { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output;
            base.InitializeComponent(core);
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
            set
            {
                this._CurrentStream = value;
                this.OnCurrentStreamChanged();
            }
        }

        protected virtual void OnCurrentStreamChanged()
        {
            if (this.CurrentStreamChanged != null)
            {
                this.CurrentStreamChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("CurrentStream");
        }

        public event EventHandler CurrentStreamChanged = delegate { };

        public void Load(string fileName, bool play)
        {
            this.Unload();
            var task = new LoadOutputStreamTask(fileName);
            task.InitializeComponent(this.Core);
            if (play)
            {
                task.Completed += (sender, e) =>
                {
                    this.CurrentStream.Play();
                };
            }
            this.OnBackgroundTask(task);
            task.Run();
        }

        public void Unload()
        {
            if (this.CurrentStream == null)
            {
                return;
            }
            this.CurrentStream.Stop();
            this.CurrentStream.Dispose();
            this.CurrentStream = null;
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
    }
}
