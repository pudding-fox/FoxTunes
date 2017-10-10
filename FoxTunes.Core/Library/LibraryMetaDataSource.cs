using FoxTunes.Interfaces;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class LibraryMetaDataSource : BaseComponent, IMetaDataSource
    {
        public LibraryMetaDataSource(LibraryItem libraryItem)
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>(libraryItem.MetaDatas);
        }

        public ObservableCollection<MetaDataItem> MetaDatas { get; private set; }
    }
}
