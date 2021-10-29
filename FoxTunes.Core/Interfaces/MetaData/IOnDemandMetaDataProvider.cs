using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IOnDemandMetaDataProvider : IStandardComponent
    {
        bool IsSourceEnabled(string name, MetaDataItemType type);

        Task<string> GetMetaData(IFileData fileData, string name, MetaDataItemType type, bool notify, object state = null);

        Task<IEnumerable<string>> GetMetaData(IEnumerable<IFileData> fileDatas, string name, MetaDataItemType type, bool notify, object state = null);

        string GetCurrentMetaData(IFileData fileData, string name, MetaDataItemType type);

        IDictionary<IFileData, string> GetCurrentMetaData(IEnumerable<IFileData> fileDatas, string name, MetaDataItemType type);

        Task SetMetaData(string name, OnDemandMetaDataValues result, MetaDataItemType type, bool notify);
    }
}
