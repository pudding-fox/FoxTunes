using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class LibraryHierarchy : ViewModelBase
    {
        public LibraryHierarchy()
        {
            this.Levels = new ObservableCollection<LibraryHierarchyLevel>();
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

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryHierarchy();
        }
    }
}
