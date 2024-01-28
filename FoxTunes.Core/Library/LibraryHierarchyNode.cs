using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    [Table(Name = "LibraryHierarchyItems")]
    public class LibraryHierarchyNode : PersistableComponent, ISelectable, IExpandable, IHierarchical
    {
        public LibraryHierarchyNode()
        {

        }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        [Column(Name = "LibraryHierarchy_Id")]
        public int LibraryHierarchyId { get; set; }

        public string Value { get; set; }

        public bool IsLeaf { get; set; }

        public LibraryHierarchyNode Parent { get; set; }

        private ResettableLazy<LibraryHierarchyNode[]> _Children { get; set; }

        public LibraryHierarchyNode[] Children
        {
            get
            {
                if (this.IsExpanded)
                {
                    if (this._Children != null)
                    {
                        return this._Children.Value;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (this.IsLeaf)
                {
                    return new LibraryHierarchyNode[] { };
                }
                else
                {
                    return new LibraryHierarchyNode[] { LibraryHierarchyNode.Empty };
                }
            }
        }

        protected virtual void OnChildrenChanged()
        {
            if (this.ChildrenChanged != null)
            {
                this.ChildrenChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Children");
        }

        public event EventHandler ChildrenChanged;

        private ResettableLazy<LibraryItem[]> _Items { get; set; }

        public LibraryItem[] Items
        {
            get
            {
                if (this._Items != null)
                {
                    return this._Items.Value;
                }
                else
                {
                    return null;
                }
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

        private bool _IsExpanded { get; set; }

        public bool IsExpanded
        {
            get
            {
                return this._IsExpanded;
            }
            set
            {
                this._IsExpanded = value;
                this.OnIsExpandedChanged();
            }
        }

        protected virtual void OnIsExpandedChanged()
        {
            this.OnChildrenChanged();
            if (this.IsExpandedChanged != null)
            {
                this.IsExpandedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsExpanded");
        }

        public event EventHandler IsExpandedChanged;

        private bool _IsSelected { get; set; }

        public bool IsSelected
        {
            get
            {
                return this._IsSelected;
            }
            set
            {
                this._IsSelected = value;
                this.OnIsSelectedChanged();
            }
        }

        protected virtual void OnIsSelectedChanged()
        {
            if (this.IsSelectedChanged != null)
            {
                this.IsSelectedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsSelected");
        }

        public event EventHandler IsSelectedChanged;

        #region IHierarchical

        IHierarchical IHierarchical.Parent
        {
            get
            {
                return this.Parent;
            }
        }

        IEnumerable<IHierarchical> IHierarchical.Children
        {
            get
            {
                return this.Children;
            }
        }

        #endregion

        public override void InitializeComponent(ICore core)
        {
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this._Children = new ResettableLazy<LibraryHierarchyNode[]>(
                () => this.LibraryHierarchyBrowser.GetNodes(this)
            );
            this._Items = new ResettableLazy<LibraryItem[]>(
                () => this.LibraryHierarchyBrowser.GetItems(this)
            );
            base.InitializeComponent(core);
        }

        public void Refresh()
        {
            this.Refresh(HierarchyDirection.Both);
        }

        protected virtual void Refresh(HierarchyDirection direction)
        {
            if (direction.HasFlag(HierarchyDirection.Up) && this.Parent != null)
            {
                this.Parent.Refresh(HierarchyDirection.Up);
            }
            //TODO: What we really want to do is refresh all {Bindings} against this instance but there isn't always a Path, sometimes it's a Converter for the entire object.
            this.OnPropertyChanged(string.Empty);
            if (direction.HasFlag(HierarchyDirection.Down) && this._Children.IsValueCreated)
            {
                foreach (var libraryHierarchyNode in this._Children.Value)
                {
                    libraryHierarchyNode.Refresh(HierarchyDirection.Down);
                }
            }
        }

        public override int GetHashCode()
        {
            //We need a hash code for this type for performance reasons.
            //base.GetHashCode() returns 0.
            return this.Id.GetHashCode() * 29;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LibraryHierarchyNode);
        }

        public static readonly LibraryHierarchyNode Empty = new LibraryHierarchyNode();

        [Flags]
        public enum HierarchyDirection : byte
        {
            None = 0,
            Up = 1,
            Down = 2,
            Both = Up | Down
        }
    }
}
