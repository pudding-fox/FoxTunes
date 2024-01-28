using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class PlaylistColumn : PersistableComponent, ISequenceableComponent
    {
        public PlaylistColumn()
        {

        }

        private int _Sequence { get; set; }

        public int Sequence
        {
            get
            {
                return this._Sequence;
            }
            set
            {
                this._Sequence = value;
                this.OnSequenceChanged();
            }
        }

        protected virtual void OnSequenceChanged()
        {
            if (this.SequenceChanged != null)
            {
                this.SequenceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Sequence");
        }

        public event EventHandler SequenceChanged;

        private string _Name { get; set; }

        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
                this.OnNameChanged();
            }
        }

        protected virtual void OnNameChanged()
        {
            if (this.NameChanged != null)
            {
                this.NameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Name");
        }

        public event EventHandler NameChanged;

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

        public event EventHandler ScriptChanged;

        private bool _IsDynamic { get; set; }

        public bool IsDynamic
        {
            get
            {
                return this._IsDynamic;
            }
            set
            {
                this._IsDynamic = value;
                this.OnIsDynamicChanged();
            }
        }

        protected virtual void OnIsDynamicChanged()
        {
            if (this.IsDynamicChanged != null)
            {
                this.IsDynamicChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsDynamic");
        }

        public event EventHandler IsDynamicChanged;

        private double? _Width { get; set; }

        public double? Width
        {
            get
            {
                return this._Width;
            }
            set
            {
                this._Width = value;
                this.OnWidthChanged();
            }
        }

        protected virtual void OnWidthChanged()
        {
            if (this.WidthChanged != null)
            {
                this.WidthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Width");
        }

        public event EventHandler WidthChanged;

        private bool _Enabled { get; set; }

        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
                this.OnEnabledChanged();
            }
        }

        protected virtual void OnEnabledChanged()
        {
            if (this.EnabledChanged != null)
            {
                this.EnabledChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Enabled");
        }

        public event EventHandler EnabledChanged;
    }
}
