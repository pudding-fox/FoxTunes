using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataSource : IBaseComponent
    {
        Task<IEnumerable<MetaDataItem>> GetMetaData(string fileName);
    }
}
