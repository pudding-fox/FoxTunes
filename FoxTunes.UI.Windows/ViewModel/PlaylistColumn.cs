using System;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class PlaylistColumn : ViewModelBase
    {
        private string _Header { get; set; }

        public string Header
        {
            get
            {
                return this._Header;
            }
            set
            {
                this._Header = value;
                this.OnHeaderChanged();
            }
        }

        protected virtual void OnHeaderChanged()
        {
            if (this.HeaderChanged != null)
            {
                this.HeaderChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Header");
        }

        public event EventHandler HeaderChanged = delegate { };

        private string _Script { get; set; }

        public string Script
        {
            get
            {
                return this._Script;
            }
            set
            {
                this._Script = value;
                this.OnScriptChanged();
            }
        }

        protected virtual void OnScriptChanged()
        {
            if (this.ScriptChanged != null)
            {
                this.ScriptChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Script");
        }

        public event EventHandler ScriptChanged = delegate { };

        protected override Freezable CreateInstanceCore()
        {
            return new PlaylistColumn();
        }
    }
}
