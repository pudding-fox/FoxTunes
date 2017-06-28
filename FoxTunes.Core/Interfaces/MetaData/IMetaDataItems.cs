using System.Collections.Generic;
using System.Collections.Specialized;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataItems : IBaseComponent, ICollection<IMetaDataItem>, INotifyCollectionChanged
    {
        int IndexOf(IMetaDataItem item);

        IMetaDataItem this[int index] { get; }

        IMetaDataItem this[string name] { get; }
    }
}
