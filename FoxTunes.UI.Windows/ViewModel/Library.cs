using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Library : ViewModelBase
    {
        public Library()
        {
            this._Items = new Dictionary<LibraryHierarchy, ObservableCollection<RenderableLibraryHierarchyItem>>();
            this._SelectedItem = new RenderableLibraryHierarchyItem();
        }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public IDataManager DataManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

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
            this.Refresh(false);
        }

        public event EventHandler SelectedHierarchyChanged = delegate { };

        private Dictionary<LibraryHierarchy, ObservableCollection<RenderableLibraryHierarchyItem>> _Items { get; set; }

        public ObservableCollection<RenderableLibraryHierarchyItem> Items
        {
            get
            {
                if (this.DataManager == null || this.SelectedHierarchy == null)
                {
                    return null;
                }
                if (!this._Items.ContainsKey(this.SelectedHierarchy))
                {
                    var libraryHierarchyItems = this.SelectedHierarchy.Items
                        .Select(libraryHierarchyItem => new RenderableLibraryHierarchyItem(libraryHierarchyItem, this.DataManager.ReadContext));
                    this._Items[this.SelectedHierarchy] = new ObservableCollection<RenderableLibraryHierarchyItem>(libraryHierarchyItems);
                }
                return this._Items[this.SelectedHierarchy];
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

        private RenderableLibraryHierarchyItem _SelectedItem { get; set; }

        public RenderableLibraryHierarchyItem SelectedItem
        {
            get
            {
                return this._SelectedItem;
            }
            set
            {
                this._SelectedItem = value;
                this.OnSelectedItemChanged();
            }
        }

        protected virtual void OnSelectedItemChanged()
        {
            if (this.SelectedItemChanged != null)
            {
                this.SelectedItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedItem");
        }

        public event EventHandler SelectedItemChanged = delegate { };

        public void Reload()
        {
            this._Items.Clear();
            this.Refresh(true);
        }

        public void Refresh(bool deep)
        {
            if (this.DataManager != null && this.SelectedHierarchy != null && deep)
            {
                this.SelectedHierarchy = this.DataManager.ReadContext.Sets.LibraryHierarchy.Find(this.SelectedHierarchy.Id);
            }
            this.OnItemsChanged();
        }

        protected override void OnCoreChanged()
        {
            this.ForegroundTaskRunner = this.Core.Components.ForegroundTaskRunner;
            this.DataManager = this.Core.Managers.Data;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Refresh(false);
            base.OnCoreChanged();
        }

        protected virtual void OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.HierarchiesUpdated:
                    this.ForegroundTaskRunner.Run(this.Reload);
                    break;
            }
        }

        private string _Filter { get; set; }

        public string Filter
        {
            get
            {
                return this._Filter;
            }
            set
            {
                this._Filter = value;
                this.OnFilterChanged();
            }
        }

        protected virtual void OnFilterChanged()
        {
            if (this.FilterChanged != null)
            {
                this.FilterChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Filter");
            this.Reload();
        }

        public event EventHandler FilterChanged = delegate { };

        protected override Freezable CreateInstanceCore()
        {
            return new Library();
        }
    }
}
