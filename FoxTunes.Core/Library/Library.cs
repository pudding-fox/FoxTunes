using FoxTunes.Interfaces;

namespace FoxTunes
{
    [Component("12B3E188-5356-4878-BF11-BD7E38BE414E", ComponentSlots.Library)]
    public class Library : StandardComponent, ILibrary
    {
        public Library()
        {

        }

        public IDatabase Database { get; private set; }

        public IDatabaseSet<LibraryItem> Set { get; private set; }

        public IDatabaseQuery<LibraryItem> Query { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.Set = this.Database.GetSet<LibraryItem>();
            this.Query = this.Database.GetQuery<LibraryItem>();
            this.Query.Include("MetaDatas");
            this.Query.Include("Properties");
            this.Query.Include("Statistics");
            base.InitializeComponent(core);
        }
    }
}
