using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IFileData : IPersistableComponent
    {
        string DirectoryName { get; }

        string FileName { get; }

        IList<MetaDataItem> MetaDatas { get; set; }
    }
}
