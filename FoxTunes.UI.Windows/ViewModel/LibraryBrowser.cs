using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibraryBrowser : Library
    {
        public LibraryBrowser()
        {
            this.SelectedItems = new Stack<LibraryHierarchyNode>();
        }

        public Stack<LibraryHierarchyNode> SelectedItems { get; private set; }

        private ObservableCollection<LibraryHierarchyNode> _Items { get; set; }

        new public ObservableCollection<LibraryHierarchyNode> Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
                this.OnItemsChanged();
            }
        }

        new protected virtual void OnItemsChanged()
        {
            if (this.ItemsChanged != null)
            {
                this.ItemsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Items");
        }

        new public event EventHandler ItemsChanged = delegate { };

        public ICommand AscendCommand
        {
            get
            {
                return new Command(() =>
                {
                    this.Ascend();
                }, () => this.SelectedItems.Count > 0);
            }
        }

        public void Ascend()
        {
            this.SelectedItems.Pop();
            this.Update();
        }

        public ICommand DescendCommand
        {
            get
            {
                return new Command<LibraryHierarchyNode>(libraryHierarchyNode =>
                {
                    this.Descend(libraryHierarchyNode);
                }, libraryHierarchyNode => libraryHierarchyNode != null);
            }
        }

        public void Descend(LibraryHierarchyNode libraryHierarchyNode)
        {
            if (!libraryHierarchyNode.IsExpanded)
            {
                libraryHierarchyNode.IsExpanded = true;
            }
            if (libraryHierarchyNode.Children.Count > 0)
            {
                this.SelectedItems.Push(libraryHierarchyNode);
            }
            this.Update();
        }

        public void Update()
        {
            if (this.SelectedItems.Count == 0)
            {
                this.Items = base.Items;
            }
            else
            {
                this.Items = this.SelectedItems.Peek().Children;
            }
        }

        public override void Refresh(bool deep)
        {
            base.Refresh(deep);
            this.SelectedItems.Clear();
            this.Update();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryBrowser();
        }
    }
}
