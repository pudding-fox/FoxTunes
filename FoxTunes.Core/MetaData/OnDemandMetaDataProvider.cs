using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class OnDemandMetaDataProvider : StandardComponent, IOnDemandMetaDataProvider
    {
        public static readonly KeyLock<string> KeyLock = new KeyLock<string>(StringComparer.OrdinalIgnoreCase);

        public OnDemandMetaDataProvider()
        {
            this.Sources = new List<IOnDemandMetaDataSource>();
        }

        public IList<IOnDemandMetaDataSource> Sources { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public IHierarchyManager HierarchyManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Sources.AddRange(ComponentRegistry.Instance.GetComponents<IOnDemandMetaDataSource>());
            this.LibraryManager = core.Managers.Library;
            this.MetaDataManager = core.Managers.MetaData;
            this.HierarchyManager = core.Managers.Hierarchy;
            base.InitializeComponent(core);
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
                        var result = await source.GetValues(queue, state).ConfigureAwait(false);
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
    }
}