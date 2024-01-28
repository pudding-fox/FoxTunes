using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibraryBrowser : LibraryBase
    {
        public LibraryBrowser()
        {
            this._Items = new Dictionary<LibraryHierarchy, Stack<ObservableCollection<LibraryHierarchyNode>>>();
            this._SelectedItem = new Dictionary<LibraryHierarchy, Stack<LibraryHierarchyNode>>();
        }

        private Dictionary<LibraryHierarchy, Stack<LibraryHierarchyNode>> _SelectedItem { get; set; }

        public LibraryHierarchyNode SelectedItem
        {
            get
            {
                if (this.SelectedHierarchy == null)
                {
                    return LibraryHierarchyNode.Empty;
                }
                if (!this._SelectedItem.ContainsKey(this.SelectedHierarchy))
                {
                    return LibraryHierarchyNode.Empty;
                }
                if (this._SelectedItem[this.SelectedHierarchy].Count == 0)
                {
                    return LibraryHierarchyNode.Empty;
                }
                return this._SelectedItem[this.SelectedHierarchy].Peek();
            }
            set
            {
                if (this.SelectedHierarchy == null || object.ReferenceEquals(this.SelectedItem, value))
                {
                    return;
                }
                this.OnSelectedItemChanging();
                if (value != null)
                {
                    this._SelectedItem[this.SelectedHierarchy].Pop();
                    this._SelectedItem[this.SelectedHierarchy].Push(value);
                    this.LibraryManager.SelectedNode = value;
                }
                this.OnSelectedItemChanged();
            }
        }

        protected virtual void OnSelectedItemChanging()
        {
            if (this.SelectedItemChanging != null)
            {
                this.SelectedItemChanging(this, EventArgs.Empty);
            }
            this.OnPropertyChanging("SelectedItem");
        }

        public event EventHandler SelectedItemChanging;

        protected virtual void OnSelectedItemChanged()
        {
            if (this.SelectedItemChanged != null)
            {
                this.SelectedItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedItem");
        }

        public event EventHandler SelectedItemChanged;

        private Dictionary<LibraryHierarchy, Stack<ObservableCollection<LibraryHierarchyNode>>> _Items { get; set; }

        public ObservableCollection<LibraryHierarchyNode> Items
        {
            get
            {
                if (this.LibraryHierarchyBrowser == null || this.SelectedHierarchy == null)
                {
                    return null;
                }
                if (!this._Items.ContainsKey(this.SelectedHierarchy))
                {
                    return null;
                }
                if (this._Items[this.SelectedHierarchy].Count == 0)
                {
                    return null;
                }
                return this._Items[this.SelectedHierarchy].Peek();
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

        public event EventHandler ItemsChanged;

        public override Task Reload()
        {
            this._Items.Clear();
            this._SelectedItem.Clear();
            return base.Reload();
        }

        public override void Refresh()
        {
            if (this.SelectedHierarchy != null)
            {
                if (!this._Items.ContainsKey(this.SelectedHierarchy))
                {
                    this._Items[this.SelectedHierarchy] = new Stack<ObservableCollection<LibraryHierarchyNode>>();
                    this._Items[this.SelectedHierarchy].Push(
                        new ObservableCollection<LibraryHierarchyNode>(this.LibraryHierarchyBrowser.GetNodes(this.SelectedHierarchy))
                    );
                }
                if (!this._SelectedItem.ContainsKey(this.SelectedHierarchy))
                {
                    this._SelectedItem[this.SelectedHierarchy] = new Stack<LibraryHierarchyNode>();
                    this._SelectedItem[this.SelectedHierarchy].Push(LibraryHierarchyNode.Empty);
                }
            }
            this.OnItemsChanged();
            this.OnSelectedItemChanged();
        }

        protected override LibraryHierarchyNode GetSelectedItem()
        {
            return this.SelectedItem;
        }

        public ICommand BrowseCommand
        {
            get
            {
                return new Command(this.Browse);
            }
        }

        public void Browse()
        {
            if (this.SelectedItem == null || LibraryHierarchyNode.Empty.Equals(this.SelectedItem))
            {
                this.Up();
            }
            else
            {
                this.Down();
            }
        }

        public void Up()
        {
            if (this._Items[this.SelectedHierarchy] == null || this._Items[this.SelectedHierarchy].Count <= 1)
            {
                return;
            }
            this._Items[this.SelectedHierarchy].Pop();
            this._SelectedItem[this.SelectedHierarchy].Pop();
            this.Refresh();
        }

        public void Down()
        {
            if (this._Items[this.SelectedHierarchy] == null || this.SelectedItem == null)
            {
                return;
            }
            this._Items[this.SelectedHierarchy].Push(
                new ObservableCollection<LibraryHierarchyNode>(new[] { LibraryHierarchyNode.Empty }.Concat(this.LibraryHierarchyBrowser.GetNodes(this.SelectedItem)))
            );
            this._SelectedItem[this.SelectedHierarchy].Push(LibraryHierarchyNode.Empty);
            this.Refresh();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryBrowser();
        }
    }
}
