using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FoxTunes.Managers
{
   public class LibraryManager: StandardManager, ILibraryManager
    {
        public ILibrary Library { get; private set; }

        public IDatabase Database { get; private set; }

        public ILibraryItemFactory LibraryItemFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Library = core.Components.Library;
            this.Database = core.Components.Database;
            this.LibraryItemFactory = core.Factories.LibraryItem;
            base.InitializeComponent(core);
        }

        public void Add(IEnumerable<string> paths)
        {

        }

        protected virtual void AddFiles(IEnumerable<string> fileNames)
        {

        }

        public void Clear()
        {
            this.Library.Items.Clear();
        }

        public ObservableCollection<LibraryItem> Items
        {
            get
            {
                return this.Library.Items;
            }
        }
    }
}
