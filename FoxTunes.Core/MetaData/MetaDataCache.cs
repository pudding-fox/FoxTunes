using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class MetaDataCache : StandardComponent, IMetaDataCache, IDisposable
    {
        public IEnumerable<MetaDataCacheKey> Keys
        {
            get
            {
                return this.Values.Keys.ToArray();
            }
        }

        public ConcurrentDictionary<MetaDataCacheKey, Lazy<IList<MetaDataItem>>> Values { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public MetaDataCache()
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
                case CommonSignals.MetaDataUpdated:
                    var names = signal.State as IEnumerable<string>;
                    if (names == null || !names.Any())
                    {
                        Logger.Write(this, LogLevel.Debug, "Meta data was updated, resetting cache.");
                        this.Reset();
                    }
                    else
                    {
                        var keys = this.Keys.Where(
                            key => !string.IsNullOrEmpty(key.MetaDataItemName) && names.Contains(key.MetaDataItemName, true)
                        );
                        foreach (var key in keys)
                        {
                            Logger.Write(this, LogLevel.Debug, "Meta data \"{0}\" was updated, evicting.", key.MetaDataItemName);
                            this.Evict(key);
                        }
                    }
                    break;
                case CommonSignals.HierarchiesUpdated:
                    if (!object.Equals(signal.State, CommonSignalFlags.SOFT))
                    {
                        Logger.Write(this, LogLevel.Debug, "Hierarchies were updated, resetting cache.");
                        this.Reset();
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Debug, "Hierarchies were updated but soft flag was specified, ignoring.");
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<MetaDataItem> GetMetaDatas(MetaDataCacheKey key, Func<IEnumerable<MetaDataItem>> factory)
        {
            return this.Values.GetOrAdd(key, _key => new Lazy<IList<MetaDataItem>>(() => new List<MetaDataItem>(factory()))).Value;
        }

        public void Reset()
        {
            this.Values = new ConcurrentDictionary<MetaDataCacheKey, Lazy<IList<MetaDataItem>>>();
        }

        public void Evict(MetaDataCacheKey key)
        {
            Logger.Write(this, LogLevel.Debug, "Evicting cache entry: {0}", key);
            this.Values.TryRemove(key);
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

        ~MetaDataCache()
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
