using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class DiscogsBehaviour : StandardBehaviour, IBackgroundTaskSource, IReportSource, IInvocableComponent, IConfigurableComponent
    {
        public const string LOOKUP_TAGS = "LLMN";

        public const string LOOKUP_ARTWORK = "LLMM";

        public ICore Core { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IOnDemandMetaDataProvider OnDemandMetaDataProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

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
                if (this.Enabled.Value)
                {
                    if (this.LibraryManager.SelectedItem != null)
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, LOOKUP_TAGS, Strings.DiscogsBehaviour_FetchTags, path: Strings.DiscogsBehaviourConfiguration_Section);
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, LOOKUP_ARTWORK, Strings.DiscogsBehaviour_FetchArtwork, path: Strings.DiscogsBehaviourConfiguration_Section);
                    }
                    if (this.PlaylistManager.SelectedItems != null && this.PlaylistManager.SelectedItems.Any())
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, LOOKUP_TAGS, Strings.DiscogsBehaviour_FetchTags, path: Strings.DiscogsBehaviourConfiguration_Section);
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, LOOKUP_ARTWORK, Strings.DiscogsBehaviour_FetchArtwork, path: Strings.DiscogsBehaviourConfiguration_Section);
                    }
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case LOOKUP_TAGS:
                    switch (component.Category)
                    {
                        case InvocationComponent.CATEGORY_LIBRARY:
                            return this.FetchTagsLibrary();
                        case InvocationComponent.CATEGORY_PLAYLIST:
                            return this.FetchTagsPlaylist();
                    }
                    break;
                case LOOKUP_ARTWORK:
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

        public async Task FetchTagsLibrary()
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
            var releaseLookups = await this.FetchTags(libraryItems).ConfigureAwait(false);
            this.OnReport(releaseLookups);
        }

        public async Task FetchTagsPlaylist()
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
            var releaseLookups = await this.FetchTags(playlistItems).ConfigureAwait(false);
            this.OnReport(releaseLookups);
        }

        public async Task<IEnumerable<Discogs.ReleaseLookup>> FetchTags(IEnumerable<IFileData> fileDatas)
        {
            var releaseLookups = Discogs.ReleaseLookup.FromFileDatas(fileDatas).ToArray();
            using (var task = new DiscogsFetchTagsTask(releaseLookups))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run().ConfigureAwait(false);
            }
            return releaseLookups;
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
                new OnDemandMetaDataRequest(
                    DiscogsFetchArtworkTask.FRONT_COVER,
                    MetaDataItemType.Tag,
                    true
                ),
                this.GetMetaDataValues(releaseLookups)
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
                new OnDemandMetaDataRequest(
                    DiscogsFetchArtworkTask.FRONT_COVER,
                    MetaDataItemType.Tag,
                    true
                ),
                this.GetMetaDataValues(releaseLookups)
            ).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Discogs.ReleaseLookup>> FetchArtwork(IEnumerable<IFileData> fileDatas)
        {
            var releaseLookups = Discogs.ReleaseLookup.FromFileDatas(fileDatas).ToArray();
            using (var task = new DiscogsFetchArtworkTask(releaseLookups))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run().ConfigureAwait(false);
            }
            return releaseLookups;
        }

        public OnDemandMetaDataValues GetMetaDataValues(IEnumerable<Discogs.ReleaseLookup> releaseLookups)
        {
            var values = new List<OnDemandMetaDataValue>();
            foreach (var releaseLookup in releaseLookups)
            {
                if (releaseLookup.Status != Discogs.ReleaseLookupStatus.Complete)
                {
                    continue;
                }
                var value = default(string);
                if (!releaseLookup.MetaData.TryGetValue(DiscogsFetchArtworkTask.FRONT_COVER, out value))
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
    }
}