using FoxTunes.Interfaces;
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

        public RenderableLibraryHierarchyItem(LibraryHierarchyItem libraryHierarchyItem, IDatabase database) : this()
        {
            this.LibraryHierarchyItem = libraryHierarchyItem;
            this.Id = libraryHierarchyItem.Id;
            this.DisplayValue = libraryHierarchyItem.DisplayValue;
            this.SortValue = libraryHierarchyItem.SortValue;
            this.IsLeaf = libraryHierarchyItem.IsLeaf;
            this.Database = database;
        }

        public LibraryHierarchyItem LibraryHierarchyItem { get; private set; }

        public override ObservableCollection<LibraryHierarchyItem> Children
        {
            get
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
                return new ObservableCollection<LibraryHierarchyItem>(sequence);
            }
        }

        public override ObservableCollection<LibraryItem> Items
        {
            get
            {
                var query = this.Database.GetMemberQuery<LibraryHierarchyItem, LibraryItem>(this.LibraryHierarchyItem, _ => _.Items);
                query.Include("MetaDatas");
                query.Include("Properties");
                query.Include("Statistics");
                return new ObservableCollection<LibraryItem>(query);
            }
        }

        public IDatabase Database { get; private set; }
    }
}
