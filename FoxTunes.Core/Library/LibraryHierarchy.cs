using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class LibraryHierarchy : PersistableComponent
    {
        public LibraryHierarchy()
        {
            this.Levels = new ObservableCollection<LibraryHierarchyLevel>();
            this.Items = new ObservableCollection<LibraryHierarchyItem>();
        }

        public LibraryHierarchy(string name) : this()
        {
            this.Name = name;
        }

        public string Name { get; set; }

        public ObservableCollection<LibraryHierarchyLevel> Levels { get; set; }

        public ObservableCollection<LibraryHierarchyItem> Items { get; set; }
    }
}
