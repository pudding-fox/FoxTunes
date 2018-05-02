using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IBassCdMetaDataSourceStrategy
    {
        bool InitializeComponent();

        IEnumerable<MetaDataItem> GetMetaDatas(int track);

        IEnumerable<MetaDataItem> GetProperties(int track);
    }
}
