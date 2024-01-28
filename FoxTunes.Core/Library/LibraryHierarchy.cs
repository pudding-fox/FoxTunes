using System;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class LibraryHierarchy : PersistableComponent, IEquatable<LibraryHierarchy>
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

        public bool Equals(LibraryHierarchy other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            return this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LibraryHierarchy);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public static bool operator ==(LibraryHierarchy a, LibraryHierarchy b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            if (object.ReferenceEquals((object)a, (object)b))
            {
                return true;
            }
            return a.Equals(b);
        }

        public static bool operator !=(LibraryHierarchy a, LibraryHierarchy b)
        {
            return !(a == b);
        }
    }
}
