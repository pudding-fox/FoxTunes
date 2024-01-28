using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    //Setting PRIORITY_HIGH so the the cache is cleared before being re-queried.
    [Component("267C58E3-8794-4377-97BF-C71118B27DB5", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class LibraryHierarchyCache : StandardComponent, ILibraryHierarchyCache, IDisposable
    {
        public bool HasItems
        {
            get
            {
                foreach (var key in this.Keys)
                {
                    var value = default(Lazy<LibraryHierarchyNode[]>);
                    if (!this.Nodes.TryGetValue(key, out value))
                    {
                        continue;
                    }
                    if (!value.IsValueCreated)
                    {
                        continue;
                    }
                    if (value.Value.Length > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public IEnumerable<LibraryHierarchyCacheKey> Keys
        {
            get
            {
                return this.Nodes.Keys.ToArray();
            }
        }

        public Lazy<LibraryHierarchy[]> Hierarchies { get; private set; }

        public ConcurrentDictionary<LibraryHierarchyCacheKey, Lazy<LibraryHierarchyNode[]>> Nodes { get; private set; }

        public IFilterParser FilterParser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public LibraryHierarchyCache()
        {
            this.Reset();
        }

        public override void InitializeComponent(ICore core)
        {
            this.FilterParser = core.Components.FilterParser;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.MetaDataUpdated:
                    var names = signal.State as IEnumerable<string>;
                    var appliesTo = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                    foreach (var key in this.Keys)
                    {
                        if (string.IsNullOrEmpty(key.Filter))
                        {
                            //No filter.
                            continue;
                        }
                        if (names != null && names.Any())
                        {
                            //If we know what meta data was updated then check if the filter applies to it.
                            if (!appliesTo.GetOrAdd(key.Filter, filter => this.FilterParser.AppliesTo(filter, names)))
                            {
                                //It's unrelated.
                                continue;
                            }
                        }
                        //The cache entry is affected by the meta data update, invalidate it.
                        this.Evict(key);
                    }
                    break;
                case CommonSignals.HierarchiesUpdated:
                    Logger.Write(this, LogLevel.Debug, "Hierarchies were updated, resetting cache.");
                    this.Reset();
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public LibraryHierarchy[] GetHierarchies(Func<IEnumerable<LibraryHierarchy>> factory)
        {
            if (this.Hierarchies == null)
            {
                this.Hierarchies = new Lazy<LibraryHierarchy[]>(() => factory().ToArray());
            }
            return this.Hierarchies.Value;
        }

        public LibraryHierarchyNode[] GetNodes(LibraryHierarchyCacheKey key, Func<IEnumerable<LibraryHierarchyNode>> factory)
        {
            return this.Nodes.GetOrAdd(key, _key => new Lazy<LibraryHierarchyNode[]>(() => factory().ToArray())).Value;
        }

        public void Reset()
        {
            this.Hierarchies = null;
            this.Nodes = new ConcurrentDictionary<LibraryHierarchyCacheKey, Lazy<LibraryHierarchyNode[]>>();
        }

        public void Evict(LibraryHierarchyCacheKey key)
        {
            Logger.Write(this, LogLevel.Debug, "Evicting cache entry: {0}", key);
            this.Nodes.TryRemove(key);
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
        }

        ~LibraryHierarchyCache()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
