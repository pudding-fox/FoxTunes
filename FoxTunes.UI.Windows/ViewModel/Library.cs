using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Library : ViewModelBase
    {
        public Library()
        {
        }

        public IDatabase Database { get; private set; }

        public IDatabaseQuery<LibraryHierarchyItem> LibraryHierarchyItemQuery { get; private set; }

        private LibraryHierarchy _SelectedHierarchy { get; set; }

        public LibraryHierarchy SelectedHierarchy
        {
            get
            {
                return this._SelectedHierarchy;
            }
            set
            {
                this._SelectedHierarchy = value;
                this.OnSelectedHierarchyChanged();
            }
        }

        protected virtual void OnSelectedHierarchyChanged()
        {
            if (this.SelectedHierarchyChanged != null)
            {
                this.SelectedHierarchyChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedHierarchy");
            this.OnItemsChanged();
        }

        public event EventHandler SelectedHierarchyChanged = delegate { };

        public ObservableCollection<RenderableLibraryHierarchyItem> Items
        {
            get
            {
                if (this.Database == null || this.SelectedHierarchy == null)
                {
                    return null;
                }
                return new ObservableCollection<RenderableLibraryHierarchyItem>(
                    this.SelectedHierarchy.Items.Select(libraryHierarchyItem => new RenderableLibraryHierarchyItem(libraryHierarchyItem, this.Database))
                );
            }
        }

        protected virtual void OnItemsChanged()
        {
            if (this.ItemsChanged != null)
            {
                this.ItemsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Items");
        }

        public event EventHandler ItemsChanged = delegate { };

        protected override void OnCoreChanged()
        {
            this.Database = this.Core.Components.Database;
            this.Core.Managers.Library.Updated += (sender, e) => this.OnItemsChanged();
            this.OnItemsChanged();
            base.OnCoreChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Library();
        }
    }
}
