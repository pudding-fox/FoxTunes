namespace FoxTunes
{
    public class LibraryHierarchyLevel : PersistableComponent
    {
        public LibraryHierarchyLevel()
        {

        }

        public LibraryHierarchyLevel(string displayScript, string sortScript) : this()
        {
            this.DisplayScript = displayScript;
            this.SortScript = sortScript;
        }

        public string DisplayScript { get; set; }

        public string SortScript { get; set; }
    }
}
