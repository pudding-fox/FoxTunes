using System;

namespace FoxTunes
{
    public class LibraryHierarchyLevel : PersistableComponent
    {
        public LibraryHierarchyLevel()
        {

        }

        public int LibraryHierarchyId { get; set; }

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

        public event EventHandler SequenceChanged = delegate { };

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
