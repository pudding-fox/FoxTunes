using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class OnDemandMetaDataProvider : StandardComponent, IOnDemandMetaDataProvider, IDisposable
    {
        const int CACHE_SIZE = 128;

        public static readonly KeyLock<string> KeyLock = new KeyLock<string>(StringComparer.OrdinalIgnoreCase);

        public OnDemandMetaDataProvider()
        {
            this.Store = new Cache(CACHE_SIZE);
            this.Sources = new List<IOnDemandMetaDataSource>();
        }

        public Cache Store { get; private set; }

        public IList<IOnDemandMetaDataSource> Sources { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public IHierarchyManager HierarchyManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Sources.AddRange(ComponentRegistry.Instance.GetComponents<IOnDemandMetaDataSource>());
            this.LibraryManager = core.Managers.Library;
            this.MetaDataManager = core.Managers.MetaData;
            this.HierarchyManager = core.Managers.Hierarchy;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                //Kill the cache if anything changes.
                case CommonSignals.LibraryUpdated:
                case CommonSignals.HierarchiesUpdated:
                case CommonSignals.PlaylistUpdated:
                case CommonSignals.MetaDataUpdated:
                case CommonSignals.ImagesUpdated:
                    this.Store.Clear();
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public bool IsSourceEnabled(string name, MetaDataItemType type)
        {
            return this.GetSources(name, type).Any();
        }

        public async Task<string> GetMetaData(IFileData fileData, string name, MetaDataItemType type, bool notify, object state = null)
        {
            var values = await this.GetMetaData(new[] { fileData }, name, type, notify, state).ConfigureAwait(false);
            return values.FirstOrDefault();
        }

        public async Task<IEnumerable<string>> GetMetaData(IEnumerable<IFileData> fileDatas, string name, MetaDataItemType type, bool notify, object state = null)
        {
            using (await KeyLock.LockAsync(name).ConfigureAwait(false))
            {
                var values = this.GetCurrentMetaData(fileDatas, name, type);
                var queue = new HashSet<IFileData>(fileDatas.Except(values.Keys));
                if (queue.Any())
                {
                    var sources = this.GetSources(name, type);
                    foreach (var source in sources)
                    {
                        //If a provider actually returns something it will be saved and returned early if this method is called again.
                        //Although the cache key is OnDemandMetaDataValues it will probably be empty.
                        //We're just caching the result so avoid repeatedly hitting APIs with the same request when nothing is returned.
                        var result = await this.Store.GetOrAdd(
                            source,
                            queue.ToArray(),
                            state,
                            () => source.GetValues(queue, state)
                        ).ConfigureAwait(false);
                        if (result != null && result.Values.Any())
                        {
                            foreach (var value in result.Values)
                            {
                                this.AddMetaData(name, value, type);
                                values[value.FileData] = value.Value;
                                queue.Remove(value.FileData);
                            }
                            this.Dispatch(() => this.SaveMetaData(name, result, type, notify));
                        }
                    }
                }
                return new HashSet<string>(values.Values, StringComparer.OrdinalIgnoreCase);
            }
        }

        public string GetCurrentMetaData(IFileData fileData, string name, MetaDataItemType type)
        {
            var values = this.GetCurrentMetaData(new[] { fileData }, name, type);
            return values.Values.FirstOrDefault();
        }

        public IDictionary<IFileData, string> GetCurrentMetaData(IEnumerable<IFileData> fileDatas, string name, MetaDataItemType type)
        {
            var values = new Dictionary<IFileData, string>();
            foreach (var fileData in fileDatas)
            {
                lock (fileData.MetaDatas)
                {
                    var metaDataItem = fileData.MetaDatas.FirstOrDefault(
                         element => string.Equals(element.Name, name, StringComparison.OrdinalIgnoreCase)
                    );
                    if (metaDataItem != null)
                    {
                        values[fileData] = metaDataItem.Value;
                    }
                }
            }
            return values;
        }

        public Task SetMetaData(string name, OnDemandMetaDataValues result, MetaDataItemType type, bool notify)
        {
            foreach (var value in result.Values)
            {
                this.AddMetaData(name, value, type);
            }
            return this.SaveMetaData(name, result, type, notify);
        }

        protected virtual IEnumerable<IOnDemandMetaDataSource> GetSources(string name, MetaDataItemType type)
        {
            foreach (var source in this.Sources)
            {
                if (source.Enabled && string.Equals(source.Name, name, StringComparison.OrdinalIgnoreCase) && source.Type == type)
                {
                    yield return source;
                }
            }
        }

        protected virtual void AddMetaData(string name, OnDemandMetaDataValue value, MetaDataItemType type)
        {
            lock (value.FileData.MetaDatas)
            {
                var metaDataItem = value.FileData.MetaDatas.FirstOrDefault(
                    element => string.Equals(element.Name, name, StringComparison.OrdinalIgnoreCase) && element.Type == type
                );
                if (metaDataItem == null)
                {
                    metaDataItem = new MetaDataItem(name, type);
                    value.FileData.MetaDatas.Add(metaDataItem);
                }
                metaDataItem.Value = value.Value;
            }
        }

        protected virtual async Task SaveMetaData(string name, OnDemandMetaDataValues result, MetaDataItemType type, bool notify)
        {
            var sources = result.Values.Select(value => value.FileData);
            var libraryItems = sources.OfType<LibraryItem>().ToArray();
            var playlistItems = sources.OfType<PlaylistItem>().ToArray();
            if (libraryItems.Any())
            {
                await this.SaveLibrary(name, libraryItems, result.Write, notify).ConfigureAwait(false);
            }
            if (playlistItems.Any())
            {
                await this.SavePlaylist(name, playlistItems, result.Write, notify).ConfigureAwait(false);
            }
        }

        protected virtual async Task SaveLibrary(string name, IEnumerable<LibraryItem> libraryItems, bool write, bool notify)
        {
            await this.MetaDataManager.Save(libraryItems, write, false, name).ConfigureAwait(false);
            if (notify)
            {
                await this.HierarchyManager.Clear(LibraryItemStatus.Import, false).ConfigureAwait(false);
                await this.HierarchyManager.Build(LibraryItemStatus.Import).ConfigureAwait(false);
                await this.LibraryManager.SetStatus(libraryItems, LibraryItemStatus.None).ConfigureAwait(false);
            }
        }

        protected virtual async Task SavePlaylist(string name, IEnumerable<PlaylistItem> playlistItems, bool write, bool notify)
        {
            await this.MetaDataManager.Save(playlistItems, write, false, name).ConfigureAwait(false);
            if (notify)
            {
                if (playlistItems.Any(playlistItem => playlistItem.LibraryItem_Id.HasValue))
                {
                    await this.HierarchyManager.Clear(LibraryItemStatus.Import, false).ConfigureAwait(false);
                    await this.HierarchyManager.Build(LibraryItemStatus.Import).ConfigureAwait(false);
                    await this.LibraryManager.SetStatus(LibraryItemStatus.None).ConfigureAwait(false);
                }
            }
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

        ~OnDemandMetaDataProvider()
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

        public class Cache
        {
            public Cache(int capacity)
            {
                this.Store = new CappedDictionary<Key, Lazy<Task<OnDemandMetaDataValues>>>(capacity);
            }

            public CappedDictionary<Key, Lazy<Task<OnDemandMetaDataValues>>> Store { get; private set; }

            public Task<OnDemandMetaDataValues> GetOrAdd(IOnDemandMetaDataSource source, IEnumerable<IFileData> fileDatas, object state, Func<Task<OnDemandMetaDataValues>> factory)
            {
                var key = new Key(source, fileDatas.ToArray(), state);
                return this.Store.GetOrAdd(key, () => new Lazy<Task<OnDemandMetaDataValues>>(factory)).Value;
            }

            public void Clear()
            {
                this.Store.Clear();
            }

            public class Key : IEquatable<Key>
            {
                public Key(IOnDemandMetaDataSource source, IFileData[] fileDatas, object state)
                {
                    this.Source = source;
                    this.FileDatas = fileDatas;
                    this.State = state;
                }

                public IOnDemandMetaDataSource Source { get; private set; }

                public IFileData[] FileDatas { get; private set; }

                public object State { get; private set; }

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
                    if (this.Source != other.Source)
                    {
                        return false;
                    }
                    if (!Enumerable.SequenceEqual(this.FileDatas, other.FileDatas))
                    {
                        return false;
                    }
                    if (this.State != other.State)
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
                        if (this.Source != null)
                        {
                            hashCode += this.Source.GetHashCode();
                        }
                        if (this.FileDatas != null)
                        {
                            foreach (var fileData in this.FileDatas)
                            {
                                hashCode += fileData.GetHashCode();
                            }
                        }
                        if (this.State != null)
                        {
                            hashCode += this.State.GetHashCode();
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
}