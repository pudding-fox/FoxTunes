using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataCache : IStandardComponent
    {
        IEnumerable<MetaDataCacheKey> Keys { get; }

        MetaDataItem[] GetMetaDatas(MetaDataCacheKey key, Func<IEnumerable<MetaDataItem>> factory);

        Task<MetaDataItem[]> GetMetaDatas(MetaDataCacheKey key, Func<Task<IEnumerable<MetaDataItem>>> factory);

        void Evict(MetaDataCacheKey key);

        void Reset();
    }

    public class MetaDataCacheKey : IEquatable<MetaDataCacheKey>
    {
        public MetaDataCacheKey(LibraryHierarchyNode libraryHierarchyNode, MetaDataItemType? metaDataItemType, string metaDataItemName, string filter)
        {
            this.LibraryHierarchyNode = libraryHierarchyNode;
            this.MetaDataItemType = metaDataItemType;
            this.MetaDataItemName = metaDataItemName;
            this.Filter = filter;
        }

        public LibraryHierarchyNode LibraryHierarchyNode { get; private set; }

        public MetaDataItemType? MetaDataItemType { get; private set; }

        public string MetaDataItemName { get; private set; }

        public string Filter { get; private set; }

        public override int GetHashCode()
        {
            var hashCode = default(int);
            unchecked
            {
                if (this.LibraryHierarchyNode != null)
                {
                    hashCode += this.LibraryHierarchyNode.GetHashCode();
                }
                if (this.MetaDataItemType.HasValue)
                {
                    hashCode += this.MetaDataItemType.Value.GetHashCode();
                }
                if (!string.IsNullOrEmpty(this.MetaDataItemName))
                {
                    hashCode += this.MetaDataItemName.ToLower().GetHashCode();
                }
                if (!string.IsNullOrEmpty(this.Filter))
                {
                    hashCode += this.Filter.ToLower().GetHashCode();
                }
            }
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as MetaDataCacheKey);
        }

        public bool Equals(MetaDataCacheKey other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (!object.Equals(this.LibraryHierarchyNode, other.LibraryHierarchyNode))
            {
                return false;
            }
            if (!object.Equals(this.MetaDataItemType, other.MetaDataItemType))
            {
                return false;
            }
            if (!string.Equals(this.MetaDataItemName, other.MetaDataItemName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (!string.Equals(this.Filter, other.Filter, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public static bool operator ==(MetaDataCacheKey a, MetaDataCacheKey b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            if (object.ReferenceEquals((object)a, (object)b))
            {
                return true;
            }
            return a.Equals(b);
        }

        public static bool operator !=(MetaDataCacheKey a, MetaDataCacheKey b)
        {
            return !(a == b);
        }
    }
}
