using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryHierarchyCache : StandardComponent, ILibraryHierarchyCache
    {
        public Lazy<IList<LibraryHierarchy>> Hierarchies { get; private set; }

        public ConcurrentDictionary<Key, Lazy<IList<LibraryHierarchyNode>>> Nodes { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public LibraryHierarchyCache()
        {
            this.Reset();
        }

        public override void InitializeComponent(ICore core)
        {
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.HierarchiesUpdated:
                    this.Reset();
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<LibraryHierarchy> GetHierarchies(Func<IEnumerable<LibraryHierarchy>> factory)
        {
            if (this.Hierarchies == null)
            {
                this.Hierarchies = new Lazy<IList<LibraryHierarchy>>(() => new List<LibraryHierarchy>(factory()));
            }
            return this.Hierarchies.Value;
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchy libraryHierarchy, string filter, Func<IEnumerable<LibraryHierarchyNode>> factory)
        {
            var key = new Key(libraryHierarchy, filter);
            return this.GetNodes(key, factory);
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(LibraryHierarchyNode libraryHierarchyNode, string filter, Func<IEnumerable<LibraryHierarchyNode>> factory)
        {
            var key = new Key(libraryHierarchyNode, filter);
            return this.GetNodes(key, factory);
        }

        public IEnumerable<LibraryHierarchyNode> GetNodes(Key key, Func<IEnumerable<LibraryHierarchyNode>> factory)
        {
            return this.Nodes.GetOrAdd(key, _key => new Lazy<IList<LibraryHierarchyNode>>(() => new List<LibraryHierarchyNode>(factory()))).Value;
        }

        public void Reset()
        {
            this.Hierarchies = null;
            this.Nodes = new ConcurrentDictionary<Key, Lazy<IList<LibraryHierarchyNode>>>();
        }

        public class Key : IEquatable<Key>
        {
            public Key(LibraryHierarchy libraryHierarchy, string filter)
            {
                this.LibraryHierarchy = libraryHierarchy;
                this.Filter = filter;
            }

            public Key(LibraryHierarchyNode libraryHierarchyNode, string filter)
            {
                this.LibraryHierarchyNode = libraryHierarchyNode;
                this.Filter = filter;
            }

            public LibraryHierarchy LibraryHierarchy { get; private set; }

            public LibraryHierarchyNode LibraryHierarchyNode { get; private set; }

            public string Filter { get; private set; }

            public virtual bool Equals(Key other)
            {
                if (other == null)
                {
                    return false;
                }
                if (object.ReferenceEquals(this, other))
                {
                    return true;
                }
                if (!object.Equals(this.LibraryHierarchy, other.LibraryHierarchy))
                {
                    return false;
                }
                if (!object.Equals(this.LibraryHierarchyNode, other.LibraryHierarchyNode))
                {
                    return false;
                }
                if (!string.Equals(this.Filter, other.Filter, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                return true;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as Key);
            }

            public override int GetHashCode()
            {
                var hashCode = default(int);
                unchecked
                {
                    if (this.LibraryHierarchy != null)
                    {
                        hashCode += this.LibraryHierarchy.GetHashCode();
                    }
                    if (this.LibraryHierarchyNode != null)
                    {
                        hashCode += this.LibraryHierarchyNode.GetHashCode();
                    }
                    if (!string.IsNullOrEmpty(this.Filter))
                    {
                        hashCode += this.Filter.GetHashCode();
                    }
                }
                return hashCode;
            }

            public static bool operator ==(Key a, Key b)
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

            public static bool operator !=(Key a, Key b)
            {
                return !(a == b);
            }
        }
    }
}
