using System;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class LibraryHierarchy : PersistableComponent
    {
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

        public ObservableCollection<LibraryHierarchyLevel> Levels { get; set; }
    }
}
