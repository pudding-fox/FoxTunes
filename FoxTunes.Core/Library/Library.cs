using FoxTunes.Interfaces;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    [Component("12B3E188-5356-4878-BF11-BD7E38BE414E", ComponentSlots.Library)]
    public class Library : StandardComponent, ILibrary
    {
        public Library()
        {

        }

        public IDatabase Database { get; private set; }

        public IPersistableSet<LibraryItem> Set { get; private set; }

        public ObservableCollection<LibraryItem> Items
        {
            get
            {
                return this.Set.AsObservable();
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.Set = this.Database.GetSet<LibraryItem>();
            this.Set.LoadCollection(item => item.MetaDatas);
            this.Set.LoadCollection(item => item.Properties);
            this.Set.LoadCollection(item => item.Statistics);
            base.InitializeComponent(core);
        }
    }
}
