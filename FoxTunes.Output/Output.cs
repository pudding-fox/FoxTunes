using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class Output : StandardComponent, IOutput
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        private bool _IsStarted { get; set; }

        public bool IsStarted
        {
            get
            {
                return this._IsStarted;
            }
        }

        protected Task SetIsStarted(bool value)
        {
            this._IsStarted = value;
            return this.OnIsStartedChanged();
        }

        protected virtual async Task OnIsStartedChanged()
        {
            if (this.IsStartedChanged != null)
            {
                var e = new AsyncEventArgs();
                this.IsStartedChanged(this, e);
                await e.Complete().ConfigureAwait(false);
            }
            this.OnPropertyChanged("IsStarted");
        }

        public event AsyncEventHandler IsStartedChanged;

        public abstract bool ShowBuffering { get; }

        public abstract IEnumerable<string> SupportedExtensions { get; }

        public abstract bool IsSupported(string fileName);

        public abstract bool IsLoaded(string fileName);

        public abstract Task Start();

        public abstract Task<IOutputStream> Load(PlaylistItem playlistItem, bool immidiate);

        public abstract IOutputStream Duplicate(IOutputStream stream);

        public abstract Task<bool> Preempt(IOutputStream stream);

        protected virtual void OnLoaded(IOutputStream stream)
        {
            if (this.Loaded != null)
            {
                this.Loaded(this, new OutputStreamEventArgs(stream));
            }
        }

        public event OutputStreamEventHandler Loaded;

        public abstract Task Unload(IOutputStream stream);

        protected virtual void OnUnloaded(IOutputStream stream)
        {
            if (this.Loaded != null)
            {
                this.Unloaded(this, new OutputStreamEventArgs(stream));
            }
        }

        public event OutputStreamEventHandler Unloaded;

        public abstract Task Shutdown();

        public abstract bool GetFormat(out int rate, out int channels, out OutputStreamFormat format);

        public abstract bool GetChannelMap(out IDictionary<int, OutputChannel> channels);

        public abstract bool CanGetData { get; }

        protected virtual void OnCanGetDataChanged()
        {
            if (this.CanGetDataChanged == null)
            {
                return;
            }
            this.CanGetDataChanged(this, EventArgs.Empty);
        }

        public event EventHandler CanGetDataChanged;

        public abstract T[] GetBuffer<T>(TimeSpan duration) where T : struct;

        public abstract int GetData(short[] buffer);

        public abstract int GetData(float[] buffer);

        public abstract float[] GetBuffer(int fftSize, bool individual = false);

        public abstract int GetData(float[] buffer, int fftSize, bool individual = false);
    }
}
