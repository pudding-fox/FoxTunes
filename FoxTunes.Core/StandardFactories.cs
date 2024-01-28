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
