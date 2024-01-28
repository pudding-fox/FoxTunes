using System;

namespace FoxTunes
{
    public class PlaylistColumn : PersistableComponent
    {
        public PlaylistColumn()
        {

        }

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

        public event EventHandler NameChanged = delegate { };

        private string _DisplayScript { get; set; }

        public string DisplayScript
        {
            get
            {
                return this._DisplayScript;
            }
            set
            {
                this._DisplayScript = value;
                this.OnDisplayScriptChanged();
            }
        }

        protected virtual void OnDisplayScriptChanged()
        {
            if (this.DisplayScriptChanged != null)
            {
                this.DisplayScriptChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("DisplayScript");
        }

        public event EventHandler DisplayScriptChanged = delegate { };

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

        public event EventHandler IsDynamicChanged = delegate { };

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

        public event EventHandler WidthChanged = delegate { };
    }
}
