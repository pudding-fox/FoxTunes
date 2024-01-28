using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxTunes.Interfaces
{
    public interface ILibraryHierarchyCache : IStandardComponent
    {
        IEnumerable<LibraryHierarchyCacheKey> Keys { get; }

        IEnumerable<LibraryHierarchy> GetHierarchies(Func<IEnumerable<LibraryHierarchy>> factory);

        IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyCacheKey key, Func<IEnumerable<LibraryHierarchyNode>> factory);

        void Evict(LibraryHierarchyCacheKey key);
    }

    public class LibraryHierarchyCacheKey : IEquatable<LibraryHierarchyCacheKey>
    {
        public LibraryHierarchyCacheKey(params object[] state)
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
            return this.Equals(obj as LibraryHierarchyCacheKey);
        }

        public bool Equals(LibraryHierarchyCacheKey other)
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

        public static bool operator ==(LibraryHierarchyCacheKey a, LibraryHierarchyCacheKey b)
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

        public static bool operator !=(LibraryHierarchyCacheKey a, LibraryHierarchyCacheKey b)
        {
            return !(a == b);
        }
    }
}
