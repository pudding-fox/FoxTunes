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

        public IPlaylistItem Create(string fileName)
        {
            var item = new PlaylistItem(fileName, this.MetaDataSourceFactory.Create(fileName));
            item.InitializeComponent(this.Core);
            return item;
        }
    }
}
