using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class StandardFactories : IStandardFactories
    {
        public IPlaylistItemFactory PlaylistItem
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IPlaylistItemFactory>();
            }
        }

        public ILibraryItemFactory LibraryItem
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<ILibraryItemFactory>();
            }
        }

        public IMetaDataSourceFactory MetaDataSource
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IMetaDataSourceFactory>();
            }
        }

        public static readonly IStandardFactories Instance = new StandardFactories();
    }
}
