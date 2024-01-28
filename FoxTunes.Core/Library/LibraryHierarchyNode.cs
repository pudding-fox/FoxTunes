using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [Table(Name = "LibraryHierarchyItems")]
    public class LibraryHierarchyNode : PersistableComponent, ISelectable, IExpandable, IHierarchical
    {
        public LibraryHierarchyNode()
        {

        }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IMetaDataBrowser MetaDataBrowser { get; private set; }

        [Column(Name = "LibraryHierarchy_Id")]
        public int LibraryHierarchyId { get; set; }

        public string Value { get; set; }

        public bool IsLeaf { get; set; }

        public LibraryHierarchyNode Parent { get; set; }

        private Lazy<LibraryHierarchyNode[]> _Children { get; set; }

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

        private Lazy<MetaDataItem[]> _MetaDatas { get; set; }

        public MetaDataItem[] MetaDatas
        {
            get
            {
                if (this._MetaDatas != null)
                {
                    return this._MetaDatas.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        protected virtual void OnMetaDatasChanged()
        {
            if (this.MetaDatasChanged != null)
            {
                this.MetaDatasChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("MetaDatas");
        }

        public event EventHandler MetaDatasChanged;

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
            this.DatabaseFactory = core.Factories.Database;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.MetaDataBrowser = core.Components.MetaDataBrowser;
            this._Children = new Lazy<LibraryHierarchyNode[]>(
                () => this.LibraryHierarchyBrowser.GetNodes(this).ToArray()
            );
            this._MetaDatas = new Lazy<MetaDataItem[]>(
                () => this.MetaDataBrowser.GetMetaDatas(this, MetaDataItemType.Image).ToArray()
            );
            base.InitializeComponent(core);
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
    }
}
