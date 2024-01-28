using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class LibraryHierarchyNode : BaseComponent
    {
        private LibraryHierarchyNode()
        {
            this.Children = new ObservableCollection<LibraryHierarchyNode>();
            this.MetaDatas = new ObservableCollection<MetaDataItem>();
        }

        public LibraryHierarchyNode(int id, string value, bool isLeaf)
        {
            this.Id = id;
            this.Value = value;
            if (this.IsLeaf = isLeaf)
            {
                this.Children = new ObservableCollection<LibraryHierarchyNode>();
            }
            else
            {
                this.Children = new ObservableCollection<LibraryHierarchyNode>(new[] { Empty });
            }
        }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public int Id { get; private set; }

        public string Value { get; private set; }

        public bool IsLeaf { get; private set; }

        private ObservableCollection<LibraryHierarchyNode> _Children { get; set; }

        public ObservableCollection<LibraryHierarchyNode> Children
        {
            get
            {
                return this._Children;
            }
            set
            {
                this._Children = value;
                this.OnChildrenChanged();
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

        public event EventHandler ChildrenChanged = delegate { };

        private ObservableCollection<MetaDataItem> _MetaDatas { get; set; }

        public ObservableCollection<MetaDataItem> MetaDatas
        {
            get
            {
                if (this._MetaDatas == null)
                {
                    this.LoadMetaDatas();
                }
                return this._MetaDatas;
            }
            set
            {
                this._MetaDatas = value;
                this.OnMetaDatasChanged();
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

        public event EventHandler MetaDatasChanged = delegate { };

        private bool _IsLoaded { get; set; }

        public bool IsLoaded
        {
            get
            {
                return this._IsLoaded;
            }
            set
            {
                this._IsLoaded = value;
                this.OnIsLoadedChanged();
            }
        }

        protected virtual void OnIsLoadedChanged()
        {
            if (this.IsLoadedChanged != null)
            {
                this.IsLoadedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsLoaded");
        }

        public event EventHandler IsLoadedChanged = delegate { };

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
            if (this.IsExpanded && !this.IsLeaf && !this.IsLoaded)
            {
                this.LoadChildren();
            }
            if (this.IsExpandedChanged != null)
            {
                this.IsExpandedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsExpanded");
        }

        public event EventHandler IsExpandedChanged = delegate { };

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

        public event EventHandler IsSelectedChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            base.InitializeComponent(core);
        }

        public void LoadChildren()
        {
            this.Children = new ObservableCollection<LibraryHierarchyNode>(this.LibraryHierarchyBrowser.GetNodes(this));
            this.IsLoaded = true;
        }

        public void LoadMetaDatas()
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>(this.LibraryHierarchyBrowser.GetMetaData(this, MetaDataItemType.Image));
        }

        public static readonly LibraryHierarchyNode Empty = new LibraryHierarchyNode();
    }
}
