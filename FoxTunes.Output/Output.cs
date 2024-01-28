using FoxTunes.Interfaces;
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
                await e.Complete();
            }
            this.OnPropertyChanged("IsStarted");
        }

        public event AsyncEventHandler IsStartedChanged;

        public abstract bool ShowBuffering { get; }

        public abstract IEnumerable<string> SupportedExtensions { get; }

        public abstract bool IsSupported(string fileName);

        public abstract Task Start();

        public abstract Task<IOutputStream> Load(PlaylistItem playlistItem, bool immidiate);

        public abstract Task<bool> Preempt(IOutputStream stream);

        public abstract Task Unload(IOutputStream stream);

        public abstract Task Shutdown();

        public abstract int GetData(float[] buffer);
    }
}
