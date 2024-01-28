using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class PlaylistItem : BaseComponent, IPlaylistItem
    {
        public PlaylistItem(string fileName, IMetaDataSource metaData)
        {
            this.FileName = fileName;
            this.MetaData = metaData;
        }

        public string FileName { get; private set; }

        public IMetaDataSource MetaData { get; private set; }
    }
}
