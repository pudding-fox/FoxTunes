using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class ImageItem : PersistableComponent
    {
        public ImageItem()
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>();
        }

        public ImageItem(string fileName) : this()
        {
            this.FileName = fileName;
        }

        public string FileName { get; set; }

        public ObservableCollection<MetaDataItem> MetaDatas { get; private set; }
    }
}
