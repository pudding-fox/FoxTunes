using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IOnDemandMetaDataSource
    {
        bool Enabled { get; }

        string Name { get; }

        MetaDataItemType Type { get; }

        bool CanGetValue(IFileData fileData, OnDemandMetaDataRequest request);

        Task<OnDemandMetaDataValues> GetValues(IEnumerable<IFileData> fileDatas, OnDemandMetaDataRequest request);
    }

    public class OnDemandMetaDataRequest
    {
        public OnDemandMetaDataRequest(string name, MetaDataItemType type, bool user, object state = null)
        {
            this.Name = name;
            this.Type = type;
            this.User = user;
            this.State = state;
        }

        public string Name { get; private set; }

        public MetaDataItemType Type { get; private set; }

        public bool User { get; private set; }

        public object State { get; private set; }
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
