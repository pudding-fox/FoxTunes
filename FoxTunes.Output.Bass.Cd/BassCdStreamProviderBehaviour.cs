using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Cd;
using ManagedBass.Gapless.Cd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("C051C82C-3391-4DDC-B856-C4BDEA86ADDC", null, priority: ComponentAttribute.PRIORITY_LOW)]
    public class BassCdStreamProviderBehaviour : StandardBehaviour, IConfigurableComponent, IBackgroundTaskSource, IInvocableComponent, IDisposable
    {
        public const string OPEN_CD = "FFFF";

        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IBassOutput Output { get; private set; }

        public CdDoorMonitor DoorMonitor { get; private set; }

        new public bool IsInitialized { get; private set; }

        public bool Enabled
        {
            get
            {
                return this.CdDrive != BassCdStreamProviderBehaviourConfiguration.CD_NO_DRIVE;
            }
        }

        private int _CdDrive { get; set; }

        public int CdDrive
        {
            get
            {
                return this._CdDrive;
            }
            set
            {
                this._CdDrive = value;
                Logger.Write(this, LogLevel.Debug, "CD Drive = {0}", this.CdDrive);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        private bool _CdLookup { get; set; }

        public bool CdLookup
        {
            get
            {
                return this._CdLookup;
            }
            private set
            {
                this._CdLookup = value;
                Logger.Write(this, LogLevel.Debug, "CD Lookup = {0}", this.CdLookup);
            }
        }

        private string _CdLookupHost { get; set; }

        public string CdLookupHost
        {
            get
            {
                return this._CdLookupHost;
            }
            private set
            {
                this._CdLookupHost = value;
                Logger.Write(this, LogLevel.Debug, "CD Lookup Host = {0}", this.CdLookupHost);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            this.DoorMonitor = ComponentRegistry.Instance.GetComponent<CdDoorMonitor>();
            this.DoorMonitor.StateChanged += this.OnStateChanged;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassCdStreamProviderBehaviourConfiguration.SECTION,
                BassCdStreamProviderBehaviourConfiguration.DRIVE_ELEMENT
            ).ConnectValue(value => this.CdDrive = BassCdStreamProviderBehaviourConfiguration.GetDrive(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassCdStreamProviderBehaviourConfiguration.SECTION,
                BassCdStreamProviderBehaviourConfiguration.LOOKUP_ELEMENT
            ).ConnectValue(value => this.CdLookup = value);
            this.Configuration.GetElement<TextConfigurationElement>(
                BassCdStreamProviderBehaviourConfiguration.SECTION,
                BassCdStreamProviderBehaviourConfiguration.LOOKUP_HOST_ELEMENT
            ).ConnectValue(value => this.CdLookupHost = value);
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            var flags = BassFlags.Decode;
            if (this.Output.Float)
            {
                flags |= BassFlags.Float;
            }
            BassUtils.OK(BassGaplessCd.Init());
            BassUtils.OK(BassGaplessCd.Enable(this.CdDrive, flags));
            BassCd.FreeOld = false;
            this.IsInitialized = true;
            Logger.Write(this, LogLevel.Debug, "BASS CD Initialized.");
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            if (!this.IsInitialized)
            {
                return;
            }
            BassGaplessCd.Disable();
            BassGaplessCd.Free();
            this.IsInitialized = false;
        }

        protected virtual void OnStateChanged(object sender, EventArgs e)
        {
            if (this.Output.IsStarted && this.DoorMonitor.State == CdDoorState.Open)
            {
                Logger.Write(this, LogLevel.Debug, "CD door was opened, shutting down the output.");
                this.Output.Shutdown();
            }
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassCdStreamProviderBehaviourConfiguration.GetConfigurationSections();
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, OPEN_CD, "Open CD");
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case OPEN_CD:
                    return this.OpenCd();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task OpenCd()
        {
            using (var task = new AddCdToPlaylistTask(this.CdDrive, this.CdLookup, this.CdLookupHost))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run();
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
            if (this.Output != null)
            {
                this.Output.Init -= this.OnInit;
                this.Output.Free -= this.OnFree;
            }
        }

        ~BassCdStreamProviderBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }

        private class AddCdToPlaylistTask : PlaylistTaskBase
        {
            public AddCdToPlaylistTask(int drive, bool cdLookup, string cdLookupHost)
            {
                this.Drive = drive;
                this.CdLookup = cdLookup;
                this.CdLookupHost = cdLookupHost;
            }

            public override bool Visible
            {
                get
                {
                    return true;
                }
            }

            public int Drive { get; private set; }

            public bool CdLookup { get; private set; }

            public string CdLookupHost { get; private set; }

            public IPlaylistManager PlaylistManager { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                this.PlaylistManager = core.Managers.Playlist;
                base.InitializeComponent(core);
            }

            protected override async Task OnRun()
            {
                if (this.Drive == BassCdStreamProviderBehaviourConfiguration.CD_NO_DRIVE)
                {
                    throw new InvalidOperationException("A valid drive must be provided.");
                }
                await this.SetName("Opening CD");
                await this.SetIsIndeterminate(true);
                try
                {
                    if (!BassCd.IsReady(this.Drive))
                    {
                        throw new InvalidOperationException("Drive is not ready.");
                    }
                    using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
                    {
                        //Always append for now.
                        this.Sequence = await this.PlaylistManager.GetInsertIndex();
                        await this.AddPlaylistItems();
                        await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset);
                        await this.AddOrUpdateMetaData();
                        await this.SetPlaylistItemsStatus(PlaylistItemStatus.None);
                    }))
                    {
                        await task.Run();
                    }
                }
                finally
                {
                    //Ignoring result on purpose.
                    BassCd.Release(this.Drive);
                }
                await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
            }

            private async Task AddPlaylistItems()
            {
                await this.SetName("Getting track list");
                await this.SetIsIndeterminate(true);
                var info = default(CDInfo);
                BassUtils.OK(BassCd.GetInfo(this.Drive, out info));
                var directoryName = string.Format("{0}:\\", info.DriveLetter);
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    using (var writer = new PlaylistWriter(this.Database, transaction))
                    {
                        for (int a = 0, b = BassCd.GetTracks(this.Drive); a < b; a++)
                        {
                            var fileName = BassCdStreamProvider.CreateUrl(this.Drive, a);
                            Logger.Write(this, LogLevel.Debug, "Adding file to playlist: {0}", fileName);
                            var playlistItem = new PlaylistItem()
                            {
                                DirectoryName = directoryName,
                                FileName = fileName,
                                Sequence = this.Sequence + a
                            };
                            await writer.Write(playlistItem);
                            this.Offset++;
                        }
                    }
                    transaction.Commit();
                }
            }

            private async Task AddOrUpdateMetaData()
            {
                Logger.Write(this, LogLevel.Debug, "Fetching meta data for new playlist items.");
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    var query = this.Database
                        .AsQueryable<PlaylistItem>(this.Database.Source(new DatabaseQueryComposer<PlaylistItem>(this.Database), transaction))
                        .Where(playlistItem => playlistItem.Status == PlaylistItemStatus.Import);
                    var info = default(CDInfo);
                    var strategy = this.GetStrategy();
                    var metaDataSource = new BassCdMetaDataSource(strategy);
                    BassUtils.OK(BassCd.GetInfo(this.Drive, out info));
                    using (var writer = new MetaDataWriter(this.Database, this.Database.Queries.AddPlaylistMetaDataItem, transaction))
                    {
                        foreach (var playlistItem in query)
                        {
                            metaDataSource.InitializeComponent(this.Core);
                            var metaData = await metaDataSource.GetMetaData(playlistItem.FileName);
                            foreach (var metaDataItem in metaData)
                            {
                                await writer.Write(playlistItem.Id, metaDataItem);
                            }
                        }
                    }
                    transaction.Commit();
                }
            }

            private IBassCdMetaDataSourceStrategy GetStrategy()
            {
                if (this.CdLookup)
                {
                    var strategy = new BassCdMetaDataSourceCddaStrategy(this.Drive, this.CdLookupHost);
                    if (strategy.InitializeComponent())
                    {
                        return strategy;
                    }
                }
                {
                    var strategy = new BassCdMetaDataSourceCdTextStrategy(this.Drive);
                    if (strategy.InitializeComponent())
                    {
                        return strategy;
                    }
                }
                return new BassCdMetaDataSourceStrategy(this.Drive);
            }
        }
    }
}
