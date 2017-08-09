using System;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class LibraryHierarchy : PersistableComponent
    {
        public LibraryHierarchy()
        {
            this.Levels = new ObservableCollection<LibraryHierarchyLevel>();
            this.Items = new ObservableCollection<LibraryHierarchyItem>();
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

        public ObservableCollection<LibraryHierarchyLevel> Levels { get; set; }

        public ObservableCollection<LibraryHierarchyItem> Items { get; set; }
    }
}
