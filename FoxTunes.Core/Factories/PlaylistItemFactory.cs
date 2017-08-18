using FoxTunes.Interfaces;

namespace FoxTunes.Factories
{
    public class PlaylistItemFactory : StandardFactory, IPlaylistItemFactory
    {
        public ICore Core { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            base.InitializeComponent(core);
        }

        public PlaylistItem Create(int sequence, string fileName)
        {
            var item = new PlaylistItem(sequence, fileName, this.MetaDataSourceFactory.Create(fileName));
            item.InitializeComponent(this.Core);
            return item;
        }

        public PlaylistItem Create(int sequence, LibraryItem libraryItem)
        {
            var item = new PlaylistItem(sequence, libraryItem.FileName, new LibraryMetaDataSource(libraryItem));
            item.InitializeComponent(this.Core);
            return item;
        }
    }
}
