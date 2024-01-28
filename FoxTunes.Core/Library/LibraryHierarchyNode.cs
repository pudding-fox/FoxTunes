using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Table(Name = "LibraryHierarchyItems")]
    public class LibraryHierarchyNode : PersistableComponent, ISelectable, IExpandable
    {
        const MetaDataItemType META_DATA_TYPE = MetaDataItemType.Image;

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        [Column(Name = "LibraryHierarchy_Id")]
        public int LibraryHierarchyId { get; set; }

        public string Value { get; set; }

        public bool IsLeaf { get; set; }

        public LibraryHierarchyNode Parent { get; set; }

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

        public event EventHandler ChildrenChanged;

        private bool _IsChildrenLoaded { get; set; }

        public bool IsChildrenLoaded
        {
            get
            {
                return this._IsChildrenLoaded;
            }
            set
            {
                this._IsChildrenLoaded = value;
                this.OnIsChildrenLoadedChanged();
            }
        }

        protected virtual void OnIsChildrenLoadedChanged()
        {
            if (this.IsChildrenLoadedChanged != null)
            {
                this.IsChildrenLoadedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsChildrenLoaded");
        }

        public event EventHandler IsChildrenLoadedChanged;

        private ObservableCollection<MetaDataItem> _MetaDatas { get; set; }

        public ObservableCollection<MetaDataItem> MetaDatas
        {
            get
            {
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

        public event EventHandler MetaDatasChanged;

        private bool _IsMetaDatasLoaded { get; set; }

        public bool IsMetaDatasLoaded
        {
            get
            {
                return this._IsMetaDatasLoaded;
            }
            set
            {
                this._IsMetaDatasLoaded = value;
                this.OnIsMetaDatasLoadedChanged();
            }
        }

        protected virtual void OnIsMetaDatasLoadedChanged()
        {
            if (this.IsMetaDatasLoadedChanged != null)
            {
                this.IsMetaDatasLoadedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsMetaDatasLoaded");
        }

        public event EventHandler IsMetaDatasLoadedChanged;

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

        public override void InitializeComponent(ICore core)
        {
            this.DatabaseFactory = core.Factories.Database;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            if (this.IsLeaf)
            {
                this.Children = new ObservableCollection<LibraryHierarchyNode>();
            }
            else
            {
                this.Children = new ObservableCollection<LibraryHierarchyNode>(new[] { Empty });
            }
            base.InitializeComponent(core);
        }

        public virtual void LoadChildren()
        {
            this.Children = new ObservableCollection<LibraryHierarchyNode>(this.LibraryHierarchyBrowser.GetNodes(this));
            this.IsChildrenLoaded = true;
        }

        public virtual Task LoadChildrenAsync()
        {
            return this.LoadChildrenAsync(CollectionLoader<LibraryHierarchyNode>.Instance);
        }

        public virtual Task LoadChildrenAsync(ICollectionLoader<LibraryHierarchyNode> collectionLoader)
        {
            return collectionLoader.Load(
                () => this.LibraryHierarchyBrowser.GetNodes(this),
                children =>
                {
                    this.Children = children;
                    this.IsChildrenLoaded = true;
                }
            );
        }

        public virtual void LoadMetaDatas()
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>(this.GetMetaDatas());
            this.IsMetaDatasLoaded = true;
        }

        public virtual Task LoadMetaDatasAsync()
        {
            return this.LoadMetaDatasAsync(CollectionLoader<MetaDataItem>.Instance);
        }

        public virtual Task LoadMetaDatasAsync(ICollectionLoader<MetaDataItem> collectionLoader)
        {
            return collectionLoader.Load(
                this.GetMetaDatas,
                metaDatas =>
                {
                    this.MetaDatas = metaDatas;
                    this.IsMetaDatasLoaded = true;
                }
            );
        }

        protected virtual IEnumerable<MetaDataItem> GetMetaDatas()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    using (var reader = MetaDataInfo.GetMetaData(database, this, META_DATA_TYPE, transaction))
                    {
                        var metaDatas = new List<MetaDataItem>();
                        foreach (var record in reader)
                        {
                            metaDatas.Add(new MetaDataItem()
                            {
                                Value = record.Get<string>("Value")
                            });
                        }
                        return metaDatas;
                    }
                }
            }
        }

        public static readonly LibraryHierarchyNode Empty = new LibraryHierarchyNode();
    }
}
