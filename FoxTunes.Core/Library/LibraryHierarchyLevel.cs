using System;

namespace FoxTunes
{
    public class LibraryHierarchyLevel : PersistableComponent
    {
        public LibraryHierarchyLevel()
        {

        }

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

        private string _SortScript { get; set; }

        public string SortScript
        {
            get
            {
                return this._SortScript;
            }
            set
            {
                this._SortScript = value;
                this.OnSortScriptChanged();
            }
        }

        protected virtual void OnSortScriptChanged()
        {
            if (this.SortScriptChanged != null)
            {
                this.SortScriptChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SortScript");
        }

        public event EventHandler SortScriptChanged = delegate { };
    }
}
