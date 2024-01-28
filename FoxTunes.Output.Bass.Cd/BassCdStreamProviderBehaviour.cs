using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Cd;
using ManagedBass.Gapless.Cd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("C051C82C-3391-4DDC-B856-C4BDEA86ADDC", null, priority: ComponentAttribute.PRIORITY_LOW)]
    [ComponentDependency(Slot = ComponentSlots.Output)]
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
            //Ignoring result on purpose.
            BassCd.Release(this.CdDrive);
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
                await task.Run().ConfigureAwait(false);
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
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
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

            public IMetaDataSource MetaDataSource { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                this.PlaylistManager = core.Managers.Playlist;
                this.MetaDataSource = new BassCdMetaDataSource(this.GetStrategy());
                this.MetaDataSource.InitializeComponent(this.Core);
                base.InitializeComponent(core);
            }

            protected override async Task OnRun()
            {
                if (this.Drive == BassCdStreamProviderBehaviourConfiguration.CD_NO_DRIVE)
                {
                    throw new InvalidOperationException("A valid drive must be provided.");
                }
                await this.SetName("Opening CD").ConfigureAwait(false);
                await this.SetIsIndeterminate(true).ConfigureAwait(false);
                try
                {
                    if (!BassCd.IsReady(this.Drive))
                    {
                        throw new InvalidOperationException("Drive is not ready.");
                    }
                    using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
                    {
                        //Always append for now.
                        this.Sequence = await this.PlaylistManager.GetInsertIndex().ConfigureAwait(false);
                        await this.AddPlaylistItems().ConfigureAwait(false);
                        await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset).ConfigureAwait(false);
                        await this.AddOrUpdateMetaData().ConfigureAwait(false);
                        await this.SetPlaylistItemsStatus(PlaylistItemStatus.None).ConfigureAwait(false);
                    }))
                    {
                        await task.Run().ConfigureAwait(false);
                    }
                }
                finally
                {
                    //Ignoring result on purpose.
                    BassCd.Release(this.Drive);
                }
                await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated)).ConfigureAwait(false);
            }

            private async Task AddPlaylistItems()
            {
                await this.SetName("Getting track list").ConfigureAwait(false);
                await this.SetIsIndeterminate(true).ConfigureAwait(false);
                var info = default(CDInfo);
                BassUtils.OK(BassCd.GetInfo(this.Drive, out info));
                var id = BassCd.GetID(this.Drive, CDID.CDPlayer);
                var directoryName = string.Format("{0}:\\", info.DriveLetter);
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    using (var writer = new PlaylistWriter(this.Database, transaction))
                    {
                        for (int a = 0, b = BassCd.GetTracks(this.Drive); a < b; a++)
                        {
                            if (BassCd.GetTrackLength(this.Drive, a) == -1)
                            {
                                //Not a music track.
                                continue;
                            }
                            var fileName = BassCdStreamProvider.CreateUrl(this.Drive, id, a);
                            fileName += string.Format("/{0}", await this.GetFileName(fileName, a));
                            Logger.Write(this, LogLevel.Debug, "Adding file to playlist: {0}", fileName);
                            var playlistItem = new PlaylistItem()
                            {
                                DirectoryName = directoryName,
                                FileName = fileName,
                                Sequence = this.Sequence + a
                            };
                            await writer.Write(playlistItem).ConfigureAwait(false);
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
                    BassUtils.OK(BassCd.GetInfo(this.Drive, out info));
                    using (var writer = new MetaDataWriter(this.Database, this.Database.Queries.AddPlaylistMetaDataItem, transaction))
                    {
                        foreach (var playlistItem in query)
                        {
                            var metaData = await this.MetaDataSource.GetMetaData(playlistItem.FileName).ConfigureAwait(false);
                            foreach (var metaDataItem in metaData)
                            {
                                await writer.Write(playlistItem.Id, metaDataItem).ConfigureAwait(false);
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

            private async Task<string> GetFileName(string fileName, int track)
            {
                var metaDatas = await this.MetaDataSource.GetMetaData(fileName).ConfigureAwait(false);
                var metaData = metaDatas.ToDictionary(
                    metaDataItem => metaDataItem.Name,
                    metaDataItem => metaDataItem.Value,
                    StringComparer.OrdinalIgnoreCase
                );
                var title = metaData.GetValueOrDefault(CommonMetaData.Title) ?? string.Empty;
                if (!string.IsNullOrEmpty(title))
                {
                    var sanitize = new Func<string, string>(value =>
                    {
                        const char PLACEHOLDER = '_';
                        var characters = Enumerable.Concat(
                            Path.GetInvalidPathChars(),
                            Path.GetInvalidFileNameChars()
                        );
                        foreach (var character in characters)
                        {
                            value = value.Replace(character, PLACEHOLDER);
                        }
                        return value;
                    });
                    return string.Format("{0:00} - {1}.cda", track + 1, sanitize(title));
                }
                else
                {
                    return string.Format("Track {0}.cda", track + 1);
                }
            }
        }
    }
}
