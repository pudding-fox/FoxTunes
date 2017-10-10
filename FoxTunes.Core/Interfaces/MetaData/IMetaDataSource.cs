using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataSource : IBaseComponent
    {
        ObservableCollection<MetaDataItem> MetaDatas { get; }
    }
}
