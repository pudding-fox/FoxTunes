using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class PlaylistItem : BaseComponent, IPlaylistItem
    {
        public PlaylistItem()
        {

        }

        public PlaylistItem(string fileName, IMetaDataSource metaData)
        {
            this.FileName = fileName;
            this.MetaData = metaData;
        }

        public Guid Id { get; set; }

        public string FileName { get; set; }

        public IMetaDataSource MetaData { get; private set; }
    }
}
