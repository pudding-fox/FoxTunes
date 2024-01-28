using FoxTunes.Interfaces;
using System.Linq;

namespace FoxTunes
{
    [Component("12B3E188-5356-4878-BF11-BD7E38BE414E", ComponentSlots.Library)]
    public class Library : StandardComponent, ILibrary
    {
        public Library()
        {

        }

        public IDatabase Database { get; private set; }

        public IDatabaseSet<LibraryItem> LibraryItemSet { get; private set; }

        public IDatabaseQuery<LibraryItem> LibraryItemQuery { get; private set; }

        public IDatabaseSet<LibraryHierarchy> LibraryHierarchySet { get; private set; }

        public IDatabaseQuery<LibraryHierarchy> LibraryHierarchyQuery { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.LibraryItemSet = this.Database.GetSet<LibraryItem>();
            this.LibraryItemQuery = this.Database.GetQuery<LibraryItem>();
            this.LibraryItemQuery.Include("MetaDatas");
            this.LibraryItemQuery.Include("Properties");
            this.LibraryItemQuery.Include("Statistics");
            this.LibraryHierarchySet = this.Database.GetSet<LibraryHierarchy>();
            this.LibraryHierarchyQuery = this.Database.GetQuery<LibraryHierarchy>();
            this.LibraryHierarchyQuery.Include("Levels");
            this.LibraryHierarchyQuery.Include("Items");
            base.InitializeComponent(core);
        }
    }
}
