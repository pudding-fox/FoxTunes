using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FoxTunes.ViewModel
{
    public class RenderableLibraryHierarchyItem : LibraryHierarchyItem
    {
        public RenderableLibraryHierarchyItem()
        {

        }

        public RenderableLibraryHierarchyItem(LibraryHierarchyItem libraryHierarchyItem, IDatabase database)
            : this()
        {
            this.LibraryHierarchyItem = libraryHierarchyItem;
            this.Database = database;
            this.Id = libraryHierarchyItem.Id;
            this.DisplayValue = libraryHierarchyItem.DisplayValue;
            this.SortValue = libraryHierarchyItem.SortValue;
            this.IsLeaf = libraryHierarchyItem.IsLeaf;

        }

        public LibraryHierarchyItem LibraryHierarchyItem { get; private set; }

        public IDatabase Database { get; private set; }

        private ObservableCollection<LibraryHierarchyItem> _Children { get; set; }

        public override ObservableCollection<LibraryHierarchyItem> Children
        {
            get
            {
                if (this._Children == null)
                {
                    var sequence = default(IEnumerable<LibraryHierarchyItem>);
                    if (this.IsLeaf)
                    {
                        sequence = Enumerable.Empty<LibraryHierarchyItem>();
                    }
                    else
                    {
                        var query = this.Database
                            .GetMemberQuery<LibraryHierarchyItem, LibraryHierarchyItem>(this.LibraryHierarchyItem, _ => _.Children)
                            .ToArray(); //We have to switch to object query as the following projection is not supported.
                        sequence = query.Select(libraryHierarchyItem => new RenderableLibraryHierarchyItem(libraryHierarchyItem, this.Database));
                    }
                    this._Children = new ObservableCollection<LibraryHierarchyItem>(sequence);
                }
                return this._Children;
            }
        }

        private ObservableCollection<LibraryItem> _Items { get; set; }

        public override ObservableCollection<LibraryItem> Items
        {
            get
            {
                if (this._Items == null)
                {
                    var query = this.Database.GetMemberQuery<LibraryHierarchyItem, LibraryItem>(this.LibraryHierarchyItem, _ => _.Items);
                    query.Include("MetaDatas");
                    query.Include("Properties");
                    query.Include("Statistics");
                    this._Items = new ObservableCollection<LibraryItem>(query);
                }
                return this._Items;
            }
        }

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

        public event EventHandler IsExpandedChanged = delegate { };
    }
}
