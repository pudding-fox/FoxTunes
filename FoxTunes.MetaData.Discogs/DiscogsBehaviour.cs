using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Text;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class DiscogsBehaviour : StandardBehaviour, IBackgroundTaskSource, IReportSource, IInvocableComponent, IConfigurableComponent
    {
        public const string LOOKUP_ARTWORK = "LLMM";

        public ICore Core { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.LibraryManager = core.Managers.Library;
            this.PlaylistManager = core.Managers.Playlist;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                DiscogsBehaviourConfiguration.SECTION,
                DiscogsBehaviourConfiguration.ENABLED
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled.Value)
                {
                    if (this.LibraryManager.SelectedItem != null)
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, LOOKUP_ARTWORK, Strings.DiscogsBehaviour_LookupArtwork, path: Strings.DiscogsBehaviourConfiguration_Section);
                    }
                    if (this.PlaylistManager.SelectedItems != null && this.PlaylistManager.SelectedItems.Any())
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, LOOKUP_ARTWORK, Strings.DiscogsBehaviour_LookupArtwork, path: Strings.DiscogsBehaviourConfiguration_Section);
                    }
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case LOOKUP_ARTWORK:
                    return this.LookupArtwork();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task LookupArtwork()
        {
            if (this.LibraryManager == null || this.LibraryManager.SelectedItem == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            //TODO: Warning: Buffering a potentially large sequence. It might be better to run the query multiple times.
            var libraryItems = this.LibraryHierarchyBrowser.GetItems(
                this.LibraryManager.SelectedItem,
                true
            ).ToArray();
            if (!libraryItems.Any())
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.LookupArtwork(libraryItems);
        }

        public async Task LookupArtwork(IFileData[] fileDatas)
        {
            var lookupItems = LookupArtworkItem.FromFileDatas(fileDatas).ToArray();
            using (var task = new LookupArtworkTask(this, lookupItems))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run().ConfigureAwait(false);
                this.OnReport(task.LookupItems);
            }
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return DiscogsBehaviourConfiguration.GetConfigurationSections();
        }

        protected virtual void OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
                return;
            }
            this.BackgroundTask(this, new BackgroundTaskEventArgs(backgroundTask));
        }

        public event BackgroundTaskEventHandler BackgroundTask;

        protected virtual void OnReport(LookupArtworkItem[] lookupItems)
        {
            var report = new LookupArtworkReport(lookupItems);
            report.InitializeComponent(this.Core);
            this.OnReport(report);
        }

        protected virtual void OnReport(IReport report)
        {
            if (this.Report == null)
            {
                return;
            }
            this.Report(this, new ReportEventArgs(report));
        }

        public event ReportEventHandler Report;

        public class LookupArtworkTask : DiscogsTask
        {
            public LookupArtworkTask(DiscogsBehaviour behaviour, LookupArtworkItem[] lookupItems)
            {
                this.Behaviour = behaviour;
                this.LookupItems = lookupItems;
            }

            public override bool Visible
            {
                get
                {
                    return true;
                }
            }

            public override bool Cancellable
            {
                get
                {
                    return true;
                }
            }

            public DiscogsBehaviour Behaviour { get; private set; }

            public LookupArtworkItem[] LookupItems { get; private set; }

            public ILibraryManager LibraryManager { get; private set; }

            public IMetaDataManager MetaDataManager { get; private set; }

            public IHierarchyManager HierarchyManager { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                this.LibraryManager = core.Managers.Library;
                this.MetaDataManager = core.Managers.MetaData;
                this.HierarchyManager = core.Managers.Hierarchy;
                base.InitializeComponent(core);
            }

            protected override Task OnStarted()
            {
                this.Name = Strings.LookupArtworkTask_Name;
                this.Position = 0;
                this.Count = this.LookupItems.Length;
                return base.OnStarted();
            }

            protected override async Task OnRun()
            {
                var position = 0;
                foreach (var lookupItem in this.LookupItems)
                {
                    this.Description = string.Format("{0} - {1}", lookupItem.Artist, lookupItem.Album);
                    this.Position = position;
                    if (this.IsCancellationRequested)
                    {
                        lookupItem.Status = LookupArtworkItemStatus.Cancelled;
                        continue;
                    }
                    if (string.IsNullOrEmpty(lookupItem.Artist) || string.IsNullOrEmpty(lookupItem.Album))
                    {
                        Logger.Write(this, LogLevel.Warn, "Cannot fetch releases, search requires at least an artist and album tag.");
                        lookupItem.AddError(Strings.LookupArtworkTask_InsufficiantData);
                        lookupItem.Status = LookupArtworkItemStatus.Failed;
                        continue;
                    }
                    try
                    {
                        lookupItem.Status = LookupArtworkItemStatus.Processing;
                        var success = await this.Lookup(lookupItem).ConfigureAwait(false);
                        if (success)
                        {
                            lookupItem.Status = LookupArtworkItemStatus.Complete;
                        }
                        else
                        {
                            lookupItem.AddError(Strings.LookupArtworkTask_NotFound);
                            lookupItem.Status = LookupArtworkItemStatus.Failed;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Write(this, LogLevel.Error, "Failed to lookup artwork: {0}", e.Message);
                        lookupItem.AddError(e.Message);
                        lookupItem.Status = LookupArtworkItemStatus.Failed;
                    }
                    position++;
                }
            }

            protected virtual async Task<bool> Lookup(LookupArtworkItem lookupItem)
            {
                Logger.Write(this, LogLevel.Debug, "Fetching releases for album: {0} - {1}", lookupItem.Artist, lookupItem.Album);
                var releases = await this.Discogs.GetReleases(lookupItem.Artist, lookupItem.Album).ConfigureAwait(false);
                Logger.Write(this, LogLevel.Debug, "Ranking releases for album: {0} - {1}", lookupItem.Artist, lookupItem.Album);
                //Get the top release by title similarity, then by largest available image.
                lookupItem.Release = releases
                    .OrderByDescending(release => release.Similarity(lookupItem.Artist, lookupItem.Album))
                    .ThenByDescending(release => release.CoverSize)
                    .ThenByDescending(release => release.ThumbSize)
                    .FirstOrDefault();
                if (lookupItem.Release != null)
                {
                    Logger.Write(this, LogLevel.Debug, "Best match for album {0} - {1}: {2}", lookupItem.Artist, lookupItem.Album, lookupItem.Release.Url);
                    var value = await this.ImportImage(
                        lookupItem,
                        lookupItem.Release.CoverUrl,
                        lookupItem.Release.ThumbUrl
                    ).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(value))
                    {
                        this.UpdateMetaData(lookupItem, value);
                        return true;
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Warn, "Failed to download artwork for album {0} - {1}: Releases don't contain images or they count not be downloaded.", lookupItem.Artist, lookupItem.Album);
                    }
                }
                else
                {
                    Logger.Write(this, LogLevel.Warn, "No matches for album {0} - {1}.", lookupItem.Artist, lookupItem.Album);
                }
                return false;
            }

            protected virtual async Task<string> ImportImage(LookupArtworkItem lookupItem, params string[] urls)
            {
                var prefix = this.GetType().Name;
                var result = default(string);
                foreach (var url in urls)
                {
                    if (!string.IsNullOrEmpty(url))
                    {
                        try
                        {
                            if (FileMetaDataStore.Exists(prefix, url, out result))
                            {
                                return result;
                            }
                            using (await KeyLock.LockAsync(url).ConfigureAwait(false))
                            {
                                if (FileMetaDataStore.Exists(prefix, url, out result))
                                {
                                    return result;
                                }
                                Logger.Write(this, LogLevel.Debug, "Downloading data from url: {0}", url);
                                var data = await this.Discogs.GetData(url).ConfigureAwait(false);
                                return await FileMetaDataStore.WriteAsync(prefix, url, data).ConfigureAwait(false);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Write(this, LogLevel.Error, "Failed to download data from url \"{0}\": {1}", url, e.Message);
                            lookupItem.AddError(e.Message);
                        }
                    }
                }
                return null;
            }

            protected virtual void UpdateMetaData(LookupArtworkItem lookupItem, string value)
            {
                var frontCover = Enum.GetName(typeof(ArtworkType), ArtworkType.FrontCover);
                foreach (var fileData in lookupItem.FileDatas)
                {
                    lock (fileData.MetaDatas)
                    {
                        bool updated = false;
                        foreach (var metaDataItem in fileData.MetaDatas)
                        {
                            if (string.Equals(metaDataItem.Name, frontCover, StringComparison.OrdinalIgnoreCase) && metaDataItem.Type == MetaDataItemType.Image)
                            {
                                metaDataItem.Value = value;
                                updated = true;
                                break;
                            }
                        }
                        if (!updated)
                        {
                            fileData.MetaDatas.Add(new MetaDataItem(frontCover, MetaDataItemType.Image)
                            {
                                Value = value
                            });
                        }
                    }
                }
            }

            protected override async Task OnCompleted()
            {
                await base.OnCompleted().ConfigureAwait(false);
                await this.SaveMetaData().ConfigureAwait(false);
            }

            protected virtual async Task SaveMetaData()
            {
                var frontCover = Enum.GetName(typeof(ArtworkType), ArtworkType.FrontCover);
                var libraryItems = new List<LibraryItem>();
                var playlistItems = new List<PlaylistItem>();
                foreach (var lookupItem in this.LookupItems)
                {
                    if (lookupItem.Status != LookupArtworkItemStatus.Complete)
                    {
                        continue;
                    }

                    libraryItems.AddRange(lookupItem.FileDatas.OfType<LibraryItem>());
                    playlistItems.AddRange(lookupItem.FileDatas.OfType<PlaylistItem>());
                }
                if (libraryItems.Any())
                {
                    await this.MetaDataManager.Save(libraryItems, true, false, frontCover).ConfigureAwait(false);
                }
                if (playlistItems.Any())
                {
                    await this.MetaDataManager.Save(playlistItems, true, false, frontCover).ConfigureAwait(false);
                }
                await this.HierarchyManager.Clear(LibraryItemStatus.Import, false).ConfigureAwait(false);
                await this.HierarchyManager.Build(LibraryItemStatus.Import).ConfigureAwait(false);
                await this.LibraryManager.SetStatus(libraryItems, LibraryItemStatus.None).ConfigureAwait(false);
            }
        }

        public class LookupArtworkItem
        {
            const int ERROR_CAPACITY = 10;

            private LookupArtworkItem()
            {
                this.Id = Guid.NewGuid();
                this._Errors = new List<string>(ERROR_CAPACITY);
            }

            public LookupArtworkItem(string artist, string album, IFileData[] fileDatas) : this()
            {
                this.Artist = artist;
                this.Album = album;
                this.FileDatas = fileDatas;
            }

            public Guid Id { get; private set; }

            public string Artist { get; private set; }

            public string Album { get; private set; }

            public IFileData[] FileDatas { get; private set; }

            public Discogs.Release Release { get; set; }

            private IList<string> _Errors { get; set; }

            public IEnumerable<string> Errors
            {
                get
                {
                    return this._Errors;
                }
            }

            public LookupArtworkItemStatus Status { get; set; }

            public void AddError(string error)
            {
                this._Errors.Add(error);
                if (this._Errors.Count > ERROR_CAPACITY)
                {
                    this._Errors.RemoveAt(0);
                }
            }

            public static IEnumerable<LookupArtworkItem> FromFileDatas(IEnumerable<IFileData> fileDatas)
            {
                return fileDatas.GroupBy(fileData =>
                {
                    var metaData = default(IDictionary<string, string>);
                    lock (fileData.MetaDatas)
                    {
                        metaData = fileData.MetaDatas.ToDictionary(
                            metaDataItem => metaDataItem.Name,
                            metaDataItem => metaDataItem.Value,
                            StringComparer.OrdinalIgnoreCase
                        );
                    }
                    return new
                    {
                        Artist = metaData.GetValueOrDefault(CommonMetaData.Artist),
                        Album = metaData.GetValueOrDefault(CommonMetaData.Album)
                    };
                }).Select(group => new LookupArtworkItem(group.Key.Artist, group.Key.Album, group.ToArray()));
            }
        }

        public enum LookupArtworkItemStatus : byte
        {
            None,
            Processing,
            Complete,
            Cancelled,
            Failed
        }

        public class LookupArtworkReport : BaseComponent, IReport
        {
            public LookupArtworkReport(IEnumerable<LookupArtworkItem> lookupItems)
            {
                this.LookupItems = lookupItems.ToDictionary(lookupItem => Guid.NewGuid());
            }

            public Dictionary<Guid, LookupArtworkItem> LookupItems { get; private set; }

            public IUserInterface UserInterface { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                this.UserInterface = core.Components.UserInterface;
                base.InitializeComponent(core);
            }

            public string Title
            {
                get
                {
                    return Strings.DiscogsBehaviour_LookupArtwork;
                }
            }

            public string Description
            {
                get
                {
                    return string.Join(
                        Environment.NewLine,
                        this.LookupItems.Values.Select(
                            lookupItem => this.GetDescription(lookupItem)
                        )
                    );
                }
            }

            protected virtual string GetDescription(LookupArtworkItem lookupItem)
            {
                var builder = new StringBuilder();
                builder.AppendFormat("{0} - {1}", lookupItem.Artist, lookupItem.Album);
                if (lookupItem.Status != LookupArtworkItemStatus.Complete && lookupItem.Errors.Any())
                {
                    builder.AppendLine(" -> Error");
                    foreach (var error in lookupItem.Errors)
                    {
                        builder.AppendLine('\t' + error);
                    }
                }
                else
                {
                    builder.AppendLine(" -> OK");
                }
                return builder.ToString();
            }

            public string[] Headers
            {
                get
                {
                    return new[]
                    {
                        Strings.LookupArtworkReport_Album,
                        Strings.LookupArtworkReport_Artist,
                        Strings.LookupArtworkReport_Status,
                        Strings.LookupArtworkReport_Release
                    };
                }
            }

            public IEnumerable<IReportRow> Rows
            {
                get
                {
                    return this.LookupItems.Select(element => new ReportRow(element.Key, element.Value));
                }
            }

            public Action<Guid> Action
            {
                get
                {
                    return key =>
                    {
                        var lookupItem = default(LookupArtworkItem);
                        if (!this.LookupItems.TryGetValue(key, out lookupItem) || lookupItem.Release == null)
                        {
                            return;
                        }
                        var url = new Uri(new Uri("https://www.discogs.com"), lookupItem.Release.Url).ToString();
                        this.UserInterface.OpenInShell(url);
                    };
                }
            }

            public class ReportRow : IReportRow
            {
                public ReportRow(Guid id, LookupArtworkItem lookupItem)
                {
                    this.Id = id;
                    this.LookupItem = lookupItem;
                }

                public Guid Id { get; private set; }

                public LookupArtworkItem LookupItem { get; private set; }

                public string[] Values
                {
                    get
                    {
                        var url = default(string);
                        if (this.LookupItem.Release != null)
                        {
                            url = this.LookupItem.Release.Url;
                        }
                        return new[]
                        {
                            this.LookupItem.Artist,
                            this.LookupItem.Album,
                            Enum.GetName(typeof(LookupArtworkItemStatus), this.LookupItem.Status),
                            url
                        };
                    }
                }
            }
        }
    }
}