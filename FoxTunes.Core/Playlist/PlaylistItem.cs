using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class PlaylistItem : PersistableComponent
    {
        public PlaylistItem()
        {

        }

        public PlaylistItem(string fileName, IMetaDataSource metaData)
        {
            this.FileName = fileName;
            this.MetaData = metaData;
        }

        public string FileName { get; set; }

        public IMetaDataSource MetaData { get; private set; }
    }
}
