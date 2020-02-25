using FoxDb.Interfaces;
using FoxTunes.Interfaces;

namespace FoxTunes.Templates
{
    public partial class LibraryHierarchyFilterBuilder
    {
        public LibraryHierarchyFilterBuilder(IDatabase database, IFilterParserResult filter, LibraryHierarchyFilterSource source)
        {
            this.Database = database;
            this.Filter = filter;
            this.Source = source;
        }

        public IDatabase Database { get; private set; }

        public IFilterParserResult Filter { get; private set; }

        public LibraryHierarchyFilterSource Source { get; private set; }
    }

    public enum LibraryHierarchyFilterSource : byte
    {
        None,
        LibraryItem,
        LibraryHierarchyItem
    }
}
