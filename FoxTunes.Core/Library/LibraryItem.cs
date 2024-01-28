using FoxTunes.Interfaces;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class LibraryItem : PersistableComponent
    {
        public LibraryItem()
        {

        }

        public LibraryItem(string fileName, IMetaDataSource metaData)
        {
            this.FileName = fileName;
            this.MetaDatas = metaData.MetaDatas;
            this.Properties = metaData.Properties;
        }

        public string FileName { get; set; }

        public ObservableCollection<MetaDataItem> MetaDatas { get; private set; }

        public ObservableCollection<PropertyItem> Properties { get; private set; }

        public ObservableCollection<StatisticItem> Statistics { get; private set; }
    }
}
