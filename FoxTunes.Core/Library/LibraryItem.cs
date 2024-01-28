using FoxTunes.Interfaces;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class LibraryItem : PersistableComponent, IMetaDataSource
    {
        public LibraryItem()
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>();
            this.Properties = new ObservableCollection<PropertyItem>();
            this.Statistics = new ObservableCollection<StatisticItem>();
        }

        public LibraryItem(string fileName, IMetaDataSource metaData) : this()
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
