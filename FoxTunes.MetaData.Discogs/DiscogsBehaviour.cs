using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class DiscogsBehaviour : StandardBehaviour, IOnDemandMetaDataSource, IBackgroundTaskSource, IReportSource, IInvocableComponent, IConfigurableComponent
    {
        private static readonly string PREFIX = typeof(DiscogsBehaviour).Name;

        public const string LOOKUP = "LLMM";

        public ICore Core { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IOnDemandMetaDataProvider OnDemandMetaDataProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement MetaData { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public BooleanConfigurationElement AutoLookup { get; private set; }

        public BooleanConfigurationElement WriteTags { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.LibraryManager = core.Managers.Library;
            this.PlaylistManager = core.Managers.Playlist;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.OnDemandMetaDataProvider = core.Components.OnDemandMetaDataProvider;
            this.Configuration = core.Components.Configuration;
            this.MetaData = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.ENABLE_ELEMENT
            );
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                DiscogsBehaviourConfiguration.SECTION,
                DiscogsBehaviourConfiguration.ENABLED
            );
            this.AutoLookup = this.Configuration.GetElement<BooleanConfigurationElement>(
                DiscogsBehaviourConfiguration.SECTION,
                DiscogsBehaviourConfiguration.AUTO_LOOKUP
            );
            this.WriteTags = this.Configuration.GetElement<BooleanConfigurationElement>(
                DiscogsBehaviourConfiguration.SECTION,
                DiscogsBehaviourConfiguration.WRITE_TAGS
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.MetaData.Value && this.Enabled.Value)
                {
                    if (this.LibraryManager.SelectedItem != null)
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, LOOKUP, Strings.DiscogsBehaviour_FetchArtwork, path: Strings.DiscogsBehaviourConfiguration_Section);
                    }
                    if (this.PlaylistManager.SelectedItems != null && this.PlaylistManager.SelectedItems.Any())
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, LOOKUP, Strings.DiscogsBehaviour_FetchArtwork, path: Strings.DiscogsBehaviourConfiguration_Section);
                    }
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case LOOKUP:
                    switch (component.Category)
                    {
                        case InvocationComponent.CATEGORY_LIBRARY:
                            return this.FetchArtworkLibrary();
                        case InvocationComponent.CATEGORY_PLAYLIST:
                            return this.FetchArtworkPlaylist();
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task FetchArtworkLibrary()
        {
            if (this.LibraryManager == null || this.LibraryManager.SelectedItem == null)
            {
                return;
            }
            //TODO: Warning: Buffering a potentially large sequence. It might be better to run the query multiple times.
            var libraryItems = this.LibraryHierarchyBrowser.GetItems(
                this.LibraryManager.SelectedItem,
                true
            ).ToArray();
            if (!libraryItems.Any())
            {
                return;
            }
            var releaseLookups = await this.FetchArtwork(libraryItems).ConfigureAwait(false);
            this.OnReport(releaseLookups);
            await this.OnDemandMetaDataProvider.SetMetaData(
                CommonMetaData.Lyrics,
                this.GetMetaDataValues(releaseLookups),
                MetaDataItemType.Tag,
                true
            ).ConfigureAwait(false);
        }

        public async Task FetchArtworkPlaylist()
        {
            if (this.PlaylistManager.SelectedItems == null)
            {
                return;
            }
            var playlistItems = this.PlaylistManager.SelectedItems.ToArray();
            if (!playlistItems.Any())
            {
                return;
            }
            var releaseLookups = await this.FetchArtwork(playlistItems).ConfigureAwait(false);
            this.OnReport(releaseLookups);
            await this.OnDemandMetaDataProvider.SetMetaData(
                CommonMetaData.Lyrics,
                this.GetMetaDataValues(releaseLookups),
                MetaDataItemType.Tag,
                true
            ).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Discogs.ReleaseLookup>> FetchArtwork(IEnumerable<IFileData> fileDatas)
        {
            var releaseLookups = Discogs.ReleaseLookup.FromFileDatas(fileDatas).ToArray();
            using (var task = new FetchArtworkTask(releaseLookups))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run().ConfigureAwait(false);
            }
            return releaseLookups;
        }

        protected virtual OnDemandMetaDataValues GetMetaDataValues(IEnumerable<Discogs.ReleaseLookup> releaseLookups)
        {
            var values = new List<OnDemandMetaDataValue>();
            foreach (var releaseLookup in releaseLookups)
            {
                if (releaseLookup.Status != Discogs.ReleaseLookupStatus.Complete)
                {
                    continue;
                }
                var value = default(string);
                if (!releaseLookup.MetaData.TryGetValue(FetchArtworkTask.FRONT_COVER, out value))
                {
                    continue;
                }
                foreach (var fileData in releaseLookup.FileDatas)
                {
                    values.Add(new OnDemandMetaDataValue(fileData, value));
                }
            }
            return new OnDemandMetaDataValues(values, this.WriteTags.Value);
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

        protected virtual void OnReport(IEnumerable<Discogs.ReleaseLookup> releaseLookups)
        {
            var report = new ReleaseLookupReport(releaseLookups);
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

        #region IOnDemandMetaDataSource

        bool IOnDemandMetaDataSource.Enabled
        {
            get
            {
                return this.Enabled.Value && this.AutoLookup.Value;
            }
        }

        string IOnDemandMetaDataSource.Name
        {
            get
            {
                return FetchArtworkTask.FRONT_COVER;
            }
        }

        MetaDataItemType IOnDemandMetaDataSource.Type
        {
            get
            {
                return MetaDataItemType.Image;
            }
        }

        async Task<OnDemandMetaDataValues> IOnDemandMetaDataSource.GetValues(IEnumerable<IFileData> fileDatas, object state)
        {
            var releaseLookups = await this.FetchArtwork(fileDatas).ConfigureAwait(false);
            return this.GetMetaDataValues(releaseLookups);
        }

        #endregion

        public class FetchArtworkTask : DiscogsLookupTask
        {
            public static readonly string FRONT_COVER = Enum.GetName(typeof(ArtworkType), ArtworkType.FrontCover);

            public FetchArtworkTask(Discogs.ReleaseLookup[] releaseLookups) : base(releaseLookups)
            {

            }

            protected override async Task<bool> OnLookupSuccess(Discogs.ReleaseLookup releaseLookup)
            {
                var value = await this.ImportImage(
                    releaseLookup,
                    releaseLookup.Release.CoverUrl,
                    releaseLookup.Release.ThumbUrl
                ).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(value))
                {
                    releaseLookup.MetaData[FRONT_COVER] = value;
                    return true;
                }
                else
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to download artwork for album {0} - {1}: Releases don't contain images or they count not be downloaded.", releaseLookup.Artist, releaseLookup.Album);
                    return false;
                }
            }

            protected virtual Task<string> ImportImage(Discogs.ReleaseLookup releaseLookup, params string[] urls)
            {
                foreach (var url in urls)
                {
                    if (!string.IsNullOrEmpty(url))
                    {
                        try
                        {
                            return FileMetaDataStore.IfNotExistsAsync(PREFIX, url, async result =>
                            {
                                Logger.Write(this, LogLevel.Debug, "Downloading data from url: {0}", url);
                                var data = await this.Discogs.GetData(url).ConfigureAwait(false);
                                return await FileMetaDataStore.WriteAsync(PREFIX, url, data).ConfigureAwait(false);
                            });
                        }
                        catch (Exception e)
                        {
                            Logger.Write(this, LogLevel.Error, "Failed to download data from url \"{0}\": {1}", url, e.Message);
                            releaseLookup.AddError(e.Message);
                        }
                    }
                }
#if NET40
                return TaskEx.FromResult(string.Empty);
#else
                return Task.FromResult(string.Empty);
#endif
            }
        }
    }
}