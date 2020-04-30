using FoxDb;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class LibraryBrowser : StandardComponent, ILibraryBrowser
    {
        public ILibraryCache LibraryCache { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryCache = core.Components.LibraryCache;
            this.DatabaseFactory = core.Factories.Database;
            base.InitializeComponent(core);
        }

        public LibraryItem Get(int id)
        {
            return this.LibraryCache.GetItem(id, () => this.GetCore(id));
        }

        private LibraryItem GetCore(int id)
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var set = database.Set<LibraryItem>(transaction);
                    return set.Find(id);
                }
            }
        }
    }
}
