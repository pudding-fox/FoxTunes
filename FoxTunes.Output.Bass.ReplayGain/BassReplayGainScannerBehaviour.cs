using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassReplayGainScannerBehaviour : StandardBehaviour, IBackgroundTaskSource, IReportSource, IInvocableComponent, IConfigurableComponent
    {
        public const string SCAN_TRACKS = "HHHH";

        public const string SCAN_ALBUMS = "IIII";

        public const string CLEAR = "JJJJ";

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public BooleanConfigurationElement WriteTags { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistManager = core.Managers.Playlist;
            this.MetaDataManager = core.Managers.MetaData;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassReplayGainBehaviourConfiguration.ENABLED
            );
            this.WriteTags = this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassReplayGainScannerBehaviourConfiguration.WRITE_TAGS
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled.Value)
                {
                    if (this.PlaylistManager.SelectedItems != null && this.PlaylistManager.SelectedItems.Any())
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, SCAN_TRACKS, "Scan Tracks", path: "Replay Gain");
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, SCAN_ALBUMS, "Scan Albums", path: "Replay Gain");
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, CLEAR, "Clear Data", path: "Replay Gain");
                    }
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case SCAN_TRACKS:
                    return this.Scan(ReplayGainMode.Track);
                case SCAN_ALBUMS:
                    return this.Scan(ReplayGainMode.Album);
                case CLEAR:
                    return this.Clear();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassReplayGainScannerBehaviourConfiguration.GetConfigurationSections();
        }

        public Task Scan(ReplayGainMode mode)
        {
            if (this.PlaylistManager.SelectedItems == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var playlistItems = this.PlaylistManager.SelectedItems.ToArray();
            if (!playlistItems.Any())
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.Scan(playlistItems, mode);
        }

        public async Task Scan(PlaylistItem[] playlistItems, ReplayGainMode mode)
        {
            using (var task = new ScanPlaylistItemsTask(this, playlistItems, mode))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run().ConfigureAwait(false);
                this.OnReport(playlistItems, task.ScannerItems);
            }
        }

        public Task Clear()
        {
            if (this.PlaylistManager.SelectedItems == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var playlistItems = this.PlaylistManager.SelectedItems.ToArray();
            if (!playlistItems.Any())
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.Clear(playlistItems);
        }

        public Task Clear(PlaylistItem[] playlistItems)
        {
            var names = new[]
            {
                CommonMetaData.ReplayGainAlbumGain,
                CommonMetaData.ReplayGainAlbumPeak,
                CommonMetaData.ReplayGainTrackGain,
                CommonMetaData.ReplayGainTrackPeak
            };
            foreach (var playlistItem in playlistItems)
            {
                lock (playlistItem.MetaDatas)
                {
                    foreach (var metaDataItem in playlistItem.MetaDatas)
                    {
                        if (!names.Contains(metaDataItem.Name, true))
                        {
                            continue;
                        }
                        //This will be converted to NaN when written.
                        metaDataItem.Value = string.Empty;
                    }
                }
            }
            return this.MetaDataManager.Save(
               playlistItems,
               this.WriteTags.Value,
               names.ToArray()
            );
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

        protected virtual void OnReport(PlaylistItem[] playlistItems, ScannerItem[] scannerItems)
        {
            var report = new BassReplayGainScannerReport(playlistItems, scannerItems);
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

        private class ScanPlaylistItemsTask : BackgroundTask
        {
            public const string ID = "1112F788-99E4-4019-84D9-55B99BD2093E";

            public static readonly IBassReplayGainScannerFactory ScannerFactory = ComponentRegistry.Instance.GetComponent<IBassReplayGainScannerFactory>();

            private ScanPlaylistItemsTask() : base(ID)
            {
                this.CancellationToken = new CancellationToken();
            }

            public ScanPlaylistItemsTask(BassReplayGainScannerBehaviour behaviour, PlaylistItem[] playlistItems, ReplayGainMode mode) : this()
            {
                this.Behaviour = behaviour;
                this.PlaylistItems = playlistItems;
                this.ScannerItems = playlistItems
                    .OrderBy(playlistItem => playlistItem.FileName)
                    .Select(playlistItem => ScannerItem.FromPlaylistItem(playlistItem, mode))
                    .ToArray();
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

            public CancellationToken CancellationToken { get; private set; }

            public BassReplayGainScannerBehaviour Behaviour { get; private set; }

            public PlaylistItem[] PlaylistItems { get; private set; }

            public ScannerItem[] ScannerItems { get; private set; }

            public ICore Core { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                this.Core = core;
                base.InitializeComponent(core);
            }

            protected override async Task OnRun()
            {
                Logger.Write(this, LogLevel.Debug, "Creating scanner.");
                using (var scanner = ScannerFactory.CreateScanner(this.ScannerItems))
                {
                    Logger.Write(this, LogLevel.Debug, "Starting scanner.");
                    using (var monitor = new BassReplayGainScannerMonitor(scanner, this.Visible, this.CancellationToken))
                    {
                        await this.WithSubTask(monitor,
                            async () => await monitor.Scan().ConfigureAwait(false)
                        ).ConfigureAwait(false);
                        this.ScannerItems = monitor.ScannerItems.Values.ToArray();

                    }
                }
                Logger.Write(this, LogLevel.Debug, "Scanner completed successfully.");
            }

            protected override async Task OnCompleted()
            {
                await base.OnCompleted().ConfigureAwait(false);
                await this.WriteTags().ConfigureAwait(false);
            }

            protected virtual async Task WriteTags()
            {
                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var scannerItem in this.ScannerItems)
                {
                    if (scannerItem.Status != ScannerItemStatus.Complete)
                    {
                        continue;
                    }
                    var playlistItem = this.GetPlaylistItem(scannerItem);
                    lock (playlistItem.MetaDatas)
                    {
                        var metaDatas = playlistItem.MetaDatas.ToDictionary(
                            element => element.Name,
                            StringComparer.OrdinalIgnoreCase
                        );
                        var metaDataItem = default(MetaDataItem);
                        if (scannerItem.GroupGain != 0)
                        {
                            if (!metaDatas.TryGetValue(CommonMetaData.ReplayGainAlbumGain, out metaDataItem))
                            {
                                metaDataItem = new MetaDataItem(CommonMetaData.ReplayGainAlbumGain, MetaDataItemType.Tag);
                                playlistItem.MetaDatas.Add(metaDataItem);
                            }
                            metaDataItem.Value = Convert.ToString(scannerItem.GroupGain);
                            names.Add(CommonMetaData.ReplayGainAlbumGain);
                        }
                        if (scannerItem.GroupPeak != 0)
                        {
                            if (!metaDatas.TryGetValue(CommonMetaData.ReplayGainAlbumPeak, out metaDataItem))
                            {
                                metaDataItem = new MetaDataItem(CommonMetaData.ReplayGainAlbumPeak, MetaDataItemType.Tag);
                                playlistItem.MetaDatas.Add(metaDataItem);
                            }
                            metaDataItem.Value = Convert.ToString(scannerItem.ItemPeak);
                            names.Add(CommonMetaData.ReplayGainAlbumPeak);
                        }
                        if (scannerItem.ItemGain != 0)
                        {
                            if (!metaDatas.TryGetValue(CommonMetaData.ReplayGainTrackGain, out metaDataItem))
                            {
                                metaDataItem = new MetaDataItem(CommonMetaData.ReplayGainTrackGain, MetaDataItemType.Tag);
                                playlistItem.MetaDatas.Add(metaDataItem);
                            }
                            metaDataItem.Value = Convert.ToString(scannerItem.ItemGain);
                            names.Add(CommonMetaData.ReplayGainTrackGain);
                        }
                        if (scannerItem.ItemPeak != 0)
                        {
                            if (!metaDatas.TryGetValue(CommonMetaData.ReplayGainTrackPeak, out metaDataItem))
                            {
                                metaDataItem = new MetaDataItem(CommonMetaData.ReplayGainTrackPeak, MetaDataItemType.Tag);
                                playlistItem.MetaDatas.Add(metaDataItem);
                            }
                            metaDataItem.Value = Convert.ToString(scannerItem.ItemPeak);
                            names.Add(CommonMetaData.ReplayGainTrackPeak);
                        }
                    }
                }
                if (!names.Any())
                {
                    //Nothing changed. Probably all tracks failed.
                    return;
                }
                await this.Behaviour.MetaDataManager.Save(
                    this.PlaylistItems,
                    this.Behaviour.WriteTags.Value,
                    names.ToArray()
                ).ConfigureAwait(false);
            }

            protected virtual PlaylistItem GetPlaylistItem(ScannerItem scannerItem)
            {
                return this.PlaylistItems.FirstOrDefault(playlistItem => string.Equals(playlistItem.FileName, scannerItem.FileName, StringComparison.OrdinalIgnoreCase));
            }

            protected override void OnCancellationRequested()
            {
                this.CancellationToken.Cancel();
                base.OnCancellationRequested();
            }
        }
    }
}
