using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface IFileData : IPersistableComponent
    {
        string DirectoryName { get; }

        string FileName { get; }

        ObservableCollection<MetaDataItem> MetaDatas { get; set; }
    }
}
