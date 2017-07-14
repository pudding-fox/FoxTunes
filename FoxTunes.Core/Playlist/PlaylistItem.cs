using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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
            this.MetaDatas = metaData.MetaDatas;
            this.Properties = metaData.Properties;
        }

        public string FileName { get; set; }

        public ObservableCollection<MetaDataItem> MetaDatas { get; private set; }

        public ObservableCollection<PropertyItem> Properties { get; private set; }
    }
}
