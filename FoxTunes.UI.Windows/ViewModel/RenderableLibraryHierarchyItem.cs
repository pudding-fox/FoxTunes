using FoxTunes.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Collections.Generic;

namespace FoxTunes.ViewModel
{
    public class RenderableLibraryHierarchyItem : LibraryHierarchyItem
    {
        public RenderableLibraryHierarchyItem()
        {

        }

        public RenderableLibraryHierarchyItem(LibraryHierarchyItem libraryHierarchyItem, IDatabase database) : this()
        {
            this.Id = libraryHierarchyItem.Id;
            this.DisplayValue = libraryHierarchyItem.DisplayValue;
            this.SortValue = libraryHierarchyItem.SortValue;
            this.ChildrenHandler = () => this.GetChildren(libraryHierarchyItem);
            this.ItemsHandler = () => this.GetItems(libraryHierarchyItem);
            this.Database = database;
        }

        public IDatabase Database { get; private set; }

        public override ObservableCollection<LibraryHierarchyItem> Children
        {
            get
            {
                return new ObservableCollection<LibraryHierarchyItem>(this.ChildrenHandler());
            }
            set
            {
                base.Children = value;
            }
        }

        public Func<IEnumerable<LibraryHierarchyItem>> ChildrenHandler { get; private set; }

        private IEnumerable<LibraryHierarchyItem> GetChildren(LibraryHierarchyItem libraryHierarchyItem)
        {
            var query =
                from item in this.Database.GetQuery<LibraryHierarchyItem>()
                where item.Parent.Id == this.Id
                select item;
            return query.ToArray().Select(item => new RenderableLibraryHierarchyItem(item, this.Database));
        }

        public override ObservableCollection<LibraryItem> Items
        {
            get
            {
                return new ObservableCollection<LibraryItem>(this.ItemsHandler());
            }
            set
            {
                base.Items = value;
            }
        }

        public Func<IEnumerable<LibraryItem>> ItemsHandler { get; private set; }

        private IEnumerable<LibraryItem> GetItems(LibraryHierarchyItem libraryHierarchyItem)
        {
            var query = this.Database.GetQuery<LibraryItem>();
            query.Include("MetaDatas");
            query.Include("Properties");
            query.Include("Statistics");
            var ids = libraryHierarchyItem.Items.Select(libraryItem => libraryItem.Id).ToArray();
            return
                from libraryItem in query
                where ids.Contains(libraryItem.Id)
                select libraryItem;
        }
    }
}
