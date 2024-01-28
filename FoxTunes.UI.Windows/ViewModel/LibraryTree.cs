using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class LibraryTree : LibraryBase
    {
        const int EXPAND_ALL_LIMIT = 5;

        public LibraryTree()
        {
            this._Items = new Dictionary<LibraryHierarchy, ObservableCollection<LibraryHierarchyNode>>();
            this._SelectedItem = new Dictionary<LibraryHierarchy, LibraryHierarchyNode>();
        }

        private Dictionary<LibraryHierarchy, LibraryHierarchyNode> _SelectedItem { get; set; }

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
                return this._SelectedItem[this.SelectedHierarchy];
            }
            set
            {
                if (this.SelectedHierarchy == null || object.ReferenceEquals(this.SelectedItem, value))
                {
                    return;
                }
                this.OnSelectedItemChanging();
                if (!this._SelectedItem.ContainsKey(this.SelectedHierarchy))
                {
                    this._SelectedItem[this.SelectedHierarchy] = LibraryHierarchyNode.Empty;
                }
                this._SelectedItem[this.SelectedHierarchy] = value;
                this.LibraryManager.SelectedNode = value;
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

        private Dictionary<LibraryHierarchy, ObservableCollection<LibraryHierarchyNode>> _Items { get; set; }

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

        public event EventHandler ItemsChanged;

        public override Task Reload()
        {
            this._Items.Clear();
            this._SelectedItem.Clear();
            return base.Reload();
        }

        public override void Refresh()
        {
            if (this.SelectedHierarchy != null && !this._Items.ContainsKey(this.SelectedHierarchy))
            {
                this._Items[this.SelectedHierarchy] = new ObservableCollection<LibraryHierarchyNode>(
                    this.LibraryHierarchyBrowser.GetNodes(this.SelectedHierarchy)
                );
            }
            this.OnItemsChanged();
            this.OnSelectedItemChanged();
        }

        protected override LibraryHierarchyNode GetSelectedItem()
        {
            return this.SelectedItem;
        }

        public void ExpandAll()
        {
            var stack = new Stack<LibraryHierarchyNode>(this.Items);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (!node.IsExpanded)
                {
                    node.IsExpanded = true;
                }
                foreach (var child in node.Children)
                {
                    if (child.IsLeaf)
                    {
                        continue;
                    }
                    stack.Push(child);
                }
            }
        }

        protected override void OnFilterChanged(object sender, EventArgs e)
        {
            this.Reload();
            if (!string.IsNullOrEmpty(this.LibraryHierarchyBrowser.Filter) && this.Items.Count <= EXPAND_ALL_LIMIT)
            {
                this.ExpandAll();
            }
            base.OnFilterChanged(sender, e);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryTree();
        }
    }
}
