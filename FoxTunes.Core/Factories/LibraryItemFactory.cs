using FoxTunes.Interfaces;

namespace FoxTunes.Factories
{
    public class LibraryItemFactory : StandardFactory, ILibraryItemFactory
    {
        public ICore Core { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            base.InitializeComponent(core);
        }

        public LibraryItem Create(string fileName)
        {
            var item = new LibraryItem(fileName, this.MetaDataSourceFactory.Create(fileName));
            item.InitializeComponent(this.Core);
            return item;
        }
    }
}
