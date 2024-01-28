namespace FoxTunes
{
    public class LibraryHierarchyItem : PersistableComponent
    {
        public LibraryHierarchyItem()
        {

        }

        public LibraryHierarchyItem(string displayValue, string sortValue, bool isLeaf) : this()
        {
            this.DisplayValue = displayValue;
            this.SortValue = sortValue;
        }

        public LibraryHierarchyItem Parent { get; set; }

        public LibraryHierarchy LibraryHierarchy { get; set; }

        public LibraryHierarchyLevel LibraryHierarchyLevel { get; set; }

        public string DisplayValue { get; set; }

        public string SortValue { get; set; }
    }
}
