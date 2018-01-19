using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class Output : StandardComponent, IOutput
    {
        private bool _IsStarted { get; set; }

        public bool IsStarted
        {
            get
            {
                return this._IsStarted;
            }
            protected set
            {
                this._IsStarted = value;
                this.OnIsStartedChanged();
            }
        }

        protected virtual void OnIsStartedChanged()
        {
            if (this.IsStartedChanged != null)
            {
                this.IsStartedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsStarted");
        }

        public event EventHandler IsStartedChanged = delegate { };

        public abstract bool IsSupported(string fileName);

        public abstract Task<IOutputStream> Load(PlaylistItem playlistItem);

        public abstract Task Preempt(IOutputStream stream);

        public abstract Task Unload(IOutputStream stream);

        public abstract Task Shutdown();
    }
}
