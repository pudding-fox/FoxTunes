using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IOnDemandMetaDataSource
    {
        bool Enabled { get; }

        string Name { get; }

        MetaDataItemType Type { get; }

        bool CanGetValue(IFileData fileData);

        Task<OnDemandMetaDataValues> GetValues(IEnumerable<IFileData> fileDatas, object state = null);
    }

    public class OnDemandMetaDataValues
    {
        public OnDemandMetaDataValues(IEnumerable<OnDemandMetaDataValue> values, bool write)
        {
            this.Values = values;
            this.Write = write;
        }

        public IEnumerable<OnDemandMetaDataValue> Values { get; private set; }

        public bool Write { get; private set; }
    }

    public class OnDemandMetaDataValue
    {
        public OnDemandMetaDataValue(IFileData fileData, string value)
        {
            this.FileData = fileData;
            this.Value = value;
        }

        public IFileData FileData { get; private set; }

        public string Value { get; private set; }
    }
}
