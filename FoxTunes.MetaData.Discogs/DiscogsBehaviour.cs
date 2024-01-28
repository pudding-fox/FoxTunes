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
        public const string FETCH_ARTWORK = "LLMM";

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
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, FETCH_ARTWORK, Strings.DiscogsBehaviour_FetchArtwork, path: Strings.DiscogsBehaviourConfiguration_Section);
                    }
                    if (this.PlaylistManager.SelectedItems != null && this.PlaylistManager.SelectedItems.Any())
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, FETCH_ARTWORK, Strings.DiscogsBehaviour_FetchArtwork, path: Strings.DiscogsBehaviourConfiguration_Section);
                    }
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case FETCH_ARTWORK:
                    return this.FetchArtwork();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task FetchArtwork()
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
            return this.FetchArtwork(libraryItems);
        }

        public async Task FetchArtwork(IFileData[] fileDatas)
        {
            var releaseLookups = Discogs.ReleaseLookup.FromFileDatas(fileDatas).ToArray();
            using (var task = new FetchArtworkTask(releaseLookups))
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

        protected virtual void OnReport(Discogs.ReleaseLookup[] releaseLookups)
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
                    this.UpdateMetaData(releaseLookup, FRONT_COVER, value, MetaDataItemType.Image);
                    return true;
                }
                else
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to download artwork for album {0} - {1}: Releases don't contain images or they count not be downloaded.", releaseLookup.Artist, releaseLookup.Album);
                    return false;
                }
            }

            protected virtual async Task<string> ImportImage(Discogs.ReleaseLookup releaseLookup, params string[] urls)
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
                            releaseLookup.AddError(e.Message);
                        }
                    }
                }
                return null;
            }

            protected override async Task OnCompleted()
            {
                await base.OnCompleted().ConfigureAwait(false);
                await this.SaveMetaData(FRONT_COVER).ConfigureAwait(false);
            }
        }

        public class ReleaseLookupReport : BaseComponent, IReport
        {
            public ReleaseLookupReport(IEnumerable<Discogs.ReleaseLookup> releaseLookups)
            {
                this.LookupItems = releaseLookups.ToDictionary(releaseLookup => Guid.NewGuid());
            }

            public Dictionary<Guid, Discogs.ReleaseLookup> LookupItems { get; private set; }

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
                    return Strings.ReleaseLookupReport_Title;
                }
            }

            public string Description
            {
                get
                {
                    return string.Join(
                        Environment.NewLine,
                        this.LookupItems.Values.Select(
                            releaseLookup => this.GetDescription(releaseLookup)
                        )
                    );
                }
            }

            protected virtual string GetDescription(Discogs.ReleaseLookup releaseLookup)
            {
                var builder = new StringBuilder();
                builder.AppendFormat("{0} - {1}", releaseLookup.Artist, releaseLookup.Album);
                if (releaseLookup.Status != Discogs.ReleaseLookupStatus.Complete && releaseLookup.Errors.Any())
                {
                    builder.AppendLine(" -> Error");
                    foreach (var error in releaseLookup.Errors)
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
                        Strings.ReleaseLookupReport_Album,
                        Strings.ReleaseLookupReport_Artist,
                        Strings.ReleaseLookupReport_Status,
                        Strings.ReleaseLookupReport_Release
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
                        var releaseLookup = default(Discogs.ReleaseLookup);
                        if (!this.LookupItems.TryGetValue(key, out releaseLookup) || releaseLookup.Release == null)
                        {
                            return;
                        }
                        var url = new Uri(new Uri("https://www.discogs.com"), releaseLookup.Release.Url).ToString();
                        this.UserInterface.OpenInShell(url);
                    };
                }
            }

            public class ReportRow : IReportRow
            {
                public ReportRow(Guid id, Discogs.ReleaseLookup releaseLookup)
                {
                    this.Id = id;
                    this.ReleaseLookup = releaseLookup;
                }

                public Guid Id { get; private set; }

                public Discogs.ReleaseLookup ReleaseLookup { get; private set; }

                public string[] Values
                {
                    get
                    {
                        var url = default(string);
                        if (this.ReleaseLookup.Release != null)
                        {
                            url = this.ReleaseLookup.Release.Url;
                        }
                        return new[]
                        {
                            this.ReleaseLookup.Artist,
                            this.ReleaseLookup.Album,
                            Enum.GetName(typeof(Discogs.ReleaseLookupStatus), this.ReleaseLookup.Status),
                            url
                        };
                    }
                }
            }
        }
    }
}