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
            this.MetaData = metaData.Items;
        }

        public string FileName { get; set; }

        public ObservableCollection<MetaDataItem> MetaData { get; private set; }

        public MetaDataItem this[string name]
        {
            get
            {
                return this.MetaData.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
