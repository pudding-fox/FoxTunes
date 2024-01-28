using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassReplayGainScannerBehaviour : StandardBehaviour, IBackgroundTaskSource, IReportSource, IInvocableComponent
    {
        public const string SCAN = "HHHH";

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistManager = core.Managers.Playlist;
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.PlaylistManager.SelectedItems != null && this.PlaylistManager.SelectedItems.Any())
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, SCAN, "Scan", path: "Replay Gain");
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case SCAN:
                    return this.Scan();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task Scan()
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
            return this.Scan(playlistItems);
        }

        public async Task Scan(PlaylistItem[] playlistItems)
        {
            using (var task = new ScanPlaylistItemsTask(this, playlistItems))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run().ConfigureAwait(false);
                this.OnReport(task.ScannerItems);
            }
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

        protected virtual void OnReport(ScannerItem[] scannerItems)
        {
            var report = new BassReplayGainScannerReport(scannerItems);
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

            public ScanPlaylistItemsTask(BassReplayGainScannerBehaviour behaviour, PlaylistItem[] playlistItems) : this()
            {
                this.Behaviour = behaviour;
                this.PlaylistItems = playlistItems;
                this.ScannerItems = playlistItems
                    .OrderBy(playlistItem => playlistItem.FileName)
                    .Select(playlistItem => ScannerItem.FromPlaylistItem(playlistItem))
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
                        monitor.StatusChanged += this.OnStatusChanged;
                        try
                        {
                            await this.WithSubTask(monitor,
                                async () => await monitor.Scan().ConfigureAwait(false)
                            ).ConfigureAwait(false);
                        }
                        finally
                        {
                            monitor.StatusChanged -= this.OnStatusChanged;
                        }
                        this.ScannerItems = monitor.ScannerItems.Values.ToArray();
                    }
                }
                Logger.Write(this, LogLevel.Debug, "Scanner completed successfully.");
            }

            protected virtual void OnStatusChanged(object sender, BassScannerMonitorEventArgs e)
            {
                if (e.ScannerItem.Status == ScannerItemStatus.Complete)
                {
                    var task = this.CopyTags(e.ScannerItem);
                }
            }

            protected virtual async Task CopyTags(ScannerItem scannerItem)
            {
                throw new NotImplementedException();
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
