using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassEncoderBehaviour : StandardBehaviour, IBackgroundTaskSource, IReportSource, IInvocableComponent, IConfigurableComponent
    {
        public const string ENCODE = "GGGG";

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Destination { get; private set; }

        public TextConfigurationElement Location { get; private set; }

        public BooleanConfigurationElement CopyTags { get; private set; }

        public IntegerConfigurationElement Threads { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistManager = core.Managers.Playlist;
            this.Configuration = core.Components.Configuration;
            this.Destination = this.Configuration.GetElement<SelectionConfigurationElement>(
                BassEncoderBehaviourConfiguration.SECTION,
                BassEncoderBehaviourConfiguration.DESTINATION_ELEMENT
            );
            this.Location = this.Configuration.GetElement<TextConfigurationElement>(
                BassEncoderBehaviourConfiguration.SECTION,
                BassEncoderBehaviourConfiguration.DESTINATION_LOCATION_ELEMENT
            );
            this.CopyTags = this.Configuration.GetElement<BooleanConfigurationElement>(
                BassEncoderBehaviourConfiguration.SECTION,
                BassEncoderBehaviourConfiguration.COPY_TAGS
            );
            this.Threads = this.Configuration.GetElement<IntegerConfigurationElement>(
                BassEncoderBehaviourConfiguration.SECTION,
                BassEncoderBehaviourConfiguration.THREADS_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, ENCODE, "Convert");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case ENCODE:
                    return this.Encode();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassEncoderBehaviourConfiguration.GetConfigurationSections();
        }

        public Task Encode()
        {
            var playlistItems = this.PlaylistManager.SelectedItems.ToArray();
            if (!playlistItems.Any())
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.Encode(playlistItems);
        }

        public async Task Encode(PlaylistItem[] playlistItems)
        {
            using (var task = new EncodePlaylistItemsTask(this, playlistItems))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run().ConfigureAwait(false);
                this.OnReport(task.EncoderItems);
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

        protected virtual void OnReport(EncoderItem[] encoderItems)
        {
            var report = new BassEncoderReport(encoderItems);
            report.InitializeComponent(this.Core);
            this.OnReport(report);
        }

        protected virtual void OnReport(IReport report)
        {
            if (this.Report == null)
            {
                return;
            }
            this.Report(this, new ReportSourceEventArgs(report));
        }

        public event ReportSourceEventHandler Report;

        private class EncodePlaylistItemsTask : BackgroundTask
        {
            public const string ID = "0E86B088-F040-444C-859D-90AD426EF33C";

            public static readonly IBassEncoderFactory EncoderFactory = ComponentRegistry.Instance.GetComponent<IBassEncoderFactory>();

            private EncodePlaylistItemsTask() : base(ID)
            {
                this.CancellationToken = new CancellationToken();
            }

            public EncodePlaylistItemsTask(BassEncoderBehaviour behaviour, PlaylistItem[] playlistItems) : this()
            {
                this.Behaviour = behaviour;
                this.PlaylistItems = playlistItems;
                this.EncoderItems = playlistItems
                    .OrderBy(playlistItem => playlistItem.FileName)
                    .Select(playlistItem => EncoderItem.FromPlaylistItem(playlistItem))
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

            public BassEncoderBehaviour Behaviour { get; private set; }

            public PlaylistItem[] PlaylistItems { get; private set; }

            public EncoderItem[] EncoderItems { get; private set; }

            public ICore Core { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                this.Core = core;
                base.InitializeComponent(core);
            }

            protected override async Task OnRun()
            {
                Logger.Write(this, LogLevel.Debug, "Creating encoder.");
                using (var encoder = EncoderFactory.CreateEncoder(this.EncoderItems))
                {
                    Logger.Write(this, LogLevel.Debug, "Starting encoder.");
                    using (var monitor = new BassEncoderMonitor(encoder, this.Visible, this.CancellationToken))
                    {
                        monitor.StatusChanged += this.OnStatusChanged;
                        try
                        {
                            await this.WithSubTask(monitor,
                                async () => await monitor.Encode().ConfigureAwait(false)
                            ).ConfigureAwait(false);
                        }
                        finally
                        {
                            monitor.StatusChanged -= this.OnStatusChanged;
                        }
                        this.EncoderItems = monitor.EncoderItems.Values.ToArray();
                    }
                }
                Logger.Write(this, LogLevel.Debug, "Encoder completed successfully.");
            }

            protected virtual void OnStatusChanged(object sender, BassEncoderMonitorEventArgs e)
            {
                if (e.EncoderItem.Status == EncoderItemStatus.Complete)
                {
                    var task = this.CopyTags(e.EncoderItem);
                }
            }

            protected virtual async Task CopyTags(EncoderItem encoderItem)
            {
                try
                {
                    Logger.Write(this, LogLevel.Debug, "Copying tags from \"{0}\" to \"{1}\".", encoderItem.InputFileName, encoderItem.OutputFileName);
                    var playlistItem = this.GetPlaylistItem(encoderItem);
                    using (var task = new WriteFileMetaDataTask(encoderItem.OutputFileName, playlistItem.MetaDatas))
                    {
                        task.InitializeComponent(this.Core);
                        await task.Run().ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to copy tags from \"{0}\" to \"{1}\": {2}", encoderItem.InputFileName, encoderItem.OutputFileName, e.Message);
                    await this.OnError(e).ConfigureAwait(false);
                }
            }

            protected virtual PlaylistItem GetPlaylistItem(EncoderItem encoderItem)
            {
                return this.PlaylistItems.FirstOrDefault(playlistItem => string.Equals(playlistItem.FileName, encoderItem.InputFileName, StringComparison.OrdinalIgnoreCase));
            }

            protected override void OnCancellationRequested()
            {
                this.CancellationToken.Cancel();
                base.OnCancellationRequested();
            }
        }
    }
}
