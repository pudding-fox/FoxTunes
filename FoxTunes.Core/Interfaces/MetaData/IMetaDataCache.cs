using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataCache : IStandardComponent
    {
        IEnumerable<MetaDataCacheKey> Keys { get; }

        IEnumerable<MetaDataItem> GetMetaDatas(MetaDataCacheKey key, Func<IEnumerable<MetaDataItem>> factory);

        void Evict(MetaDataCacheKey key);

        void Reset();
    }

    public class MetaDataCacheKey : IEquatable<MetaDataCacheKey>
    {
        public MetaDataCacheKey(params object[] state)
        {
            this.State = state;
        }

        public object[] State { get; private set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var state in this.State)
            {
                if (state == null)
                {
                    continue;
                }
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }
                builder.AppendFormat("{0} = {1}", state.GetType().Name, state);
            }
            return builder.ToString();
        }

        public override int GetHashCode()
        {
            var hashCode = default(int);
            unchecked
            {
                foreach (var state in this.State)
                {
                    if (state == null)
                    {
                        continue;
                    }
                    hashCode += state.GetHashCode();
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
            return Enumerable.SequenceEqual(this.State, other.State);
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
