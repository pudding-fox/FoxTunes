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

        public abstract IEnumerable<string> SupportedExtensions { get; }

        public abstract bool IsSupported(string fileName);

        public abstract Task Start();

        public abstract Task<IOutputStream> Load(PlaylistItem playlistItem, bool immidiate);

        public abstract Task<bool> Preempt(IOutputStream stream);

        public abstract Task Unload(IOutputStream stream);

        public abstract Task Shutdown();
    }
}
