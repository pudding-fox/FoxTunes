using FoxTunes.Interfaces;
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
            this.Database = database;
        }

        public LibraryHierarchyItem LibraryHierarchyItem { get; private set; }

        public override ObservableCollection<LibraryHierarchyItem> Children
        {
            get
            {
                var query = this.Database.GetMemberQuery<LibraryHierarchyItem, LibraryHierarchyItem>(this.LibraryHierarchyItem, _ => _.Children);
                return new ObservableCollection<LibraryHierarchyItem>(query.ToArray().Select(
                    libraryHierarchyItem => new RenderableLibraryHierarchyItem(libraryHierarchyItem, this.Database)
                ));
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
