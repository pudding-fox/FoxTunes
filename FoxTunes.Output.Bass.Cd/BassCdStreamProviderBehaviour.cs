using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Cd;
using ManagedBass.Gapless;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("C051C82C-3391-4DDC-B856-C4BDEA86ADDC", null, priority: ComponentAttribute.PRIORITY_LOW)]
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassCdStreamProviderBehaviour : StandardBehaviour, IConfigurableComponent, IInvocableComponent, IDisposable
    {
        public const string OPEN_CD = "FFFF";

        public static string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(BassCdStreamProviderBehaviour).Assembly.Location);
            }
        }

        public BassCdStreamProviderBehaviour()
        {
            this.CurrentDrive = CdUtils.NO_DRIVE;
            BassPluginLoader.AddPath(Path.Combine(Location, "Addon"));
            BassPluginLoader.AddPath(Path.Combine(Loader.FolderName, "bass_gapless_cd.dll"));
        }

        public int CurrentDrive { get; private set; }

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IBackgroundTaskEmitter BackgroundTaskEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IBassOutput Output { get; private set; }

        public CdDoorMonitor DoorMonitor { get; private set; }

        public IBassStreamPipelineFactory BassStreamPipelineFactory { get; private set; }

        private bool _Enabled { get; set; }

        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
                Logger.Write(this, LogLevel.Debug, "Enabled = {0}", this.Enabled);
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
            this.Output.Free += this.OnFree;
            this.PlaylistManager = core.Managers.Playlist;
            this.BackgroundTaskEmitter = core.Components.BackgroundTaskEmitter;
            this.DoorMonitor = ComponentRegistry.Instance.GetComponent<CdDoorMonitor>();
            this.DoorMonitor.StateChanged += this.OnStateChanged;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassCdStreamProviderBehaviourConfiguration.SECTION,
                BassCdStreamProviderBehaviourConfiguration.ENABLED_ELEMENT
            ).ConnectValue(value => this.Enabled = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassCdStreamProviderBehaviourConfiguration.SECTION,
                BassCdStreamProviderBehaviourConfiguration.LOOKUP_ELEMENT
            ).ConnectValue(value => this.CdLookup = value);
            this.Configuration.GetElement<TextConfigurationElement>(
                BassCdStreamProviderBehaviourConfiguration.SECTION,
                BassCdStreamProviderBehaviourConfiguration.LOOKUP_HOST_ELEMENT
            ).ConnectValue(value => this.CdLookupHost = value);
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            this.BassStreamPipelineFactory.CreatingPipeline += this.OnCreatingPipeline;
            base.InitializeComponent(core);
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            if (this.CurrentDrive == CdUtils.NO_DRIVE)
            {
                return;
            }
            try
            {
                BassGapless.Cd.Disable();
                //Ignoring result on purpose.
                BassCd.Release(this.CurrentDrive);
                Logger.Write(this, LogLevel.Debug, "Gapless CD playback was disabled.");
            }
            catch (Exception ex)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to disable gapless CD playback: {0}", ex.Message);
            }
            this.CurrentDrive = CdUtils.NO_DRIVE;
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            var drive = default(int);
            var id = default(string);
            var track = default(int);
            if (!CdUtils.ParseUrl(e.Stream.FileName, out drive, out id, out track))
            {
                return;
            }
            var flags = BassFlags.Decode;
            if (e.Stream.Flags.HasFlag(BassFlags.Float))
            {
                flags |= BassFlags.Float;
            }
            try
            {
                if (BassGapless.Cd.Enable(drive, flags))
                {
                    Logger.Write(this, LogLevel.Debug, "Gapless CD playback was enabled.");
                    this.CurrentDrive = drive;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to enable gapless CD playback: {0}", ex.Message);
            }
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
                    foreach (var drive in CdUtils.GetDrives())
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, OPEN_CD, drive, path: Strings.BassCdStreamProviderBehaviour_Path);
                    }
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case OPEN_CD:
                    var drive = CdUtils.GetDrive(component.Name);
                    if (drive == CdUtils.NO_DRIVE)
                    {
                        break;
                    }
                    return this.OpenCd(drive);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public Task OpenCd(int drive)
        {
            return this.OpenCd(this.PlaylistManager.SelectedPlaylist, drive);
        }

        public async Task OpenCd(Playlist playlist, int drive)
        {
            using (var task = new AddCdToPlaylistTask(playlist, drive, this.CdLookup, this.CdLookupHost))
            {
                task.InitializeComponent(this.Core);
                await this.BackgroundTaskEmitter.Send(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

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
                this.Output.Free -= this.OnFree;
            }
            if (this.BassStreamPipelineFactory != null)
            {
                this.BassStreamPipelineFactory.CreatingPipeline -= this.OnCreatingPipeline;
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
            public AddCdToPlaylistTask(Playlist playlist, int drive, bool cdLookup, string cdLookupHost) : base(playlist)
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

            public IPlaylistBrowser PlaylistBrowser { get; private set; }

            public CdPlaylistItemFactory Factory { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                this.PlaylistManager = core.Managers.Playlist;
                this.PlaylistBrowser = core.Components.PlaylistBrowser;
                this.Factory = new CdPlaylistItemFactory(this.Drive, this.CdLookup, this.CdLookupHost);
                this.Factory.InitializeComponent(core);
                base.InitializeComponent(core);
            }

            protected override async Task OnRun()
            {
                this.Name = "Opening CD";
                try
                {
                    if (!BassCd.IsReady(this.Drive))
                    {
                        throw new InvalidOperationException("Drive is not ready.");
                    }
                    var playlist = this.PlaylistManager.SelectedPlaylist;
                    var playlistItems = await this.Factory.Create().ConfigureAwait(false);
                    using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
                    {
                        //Always append for now.
                        this.Sequence = this.PlaylistBrowser.GetInsertIndex(this.PlaylistManager.SelectedPlaylist);
                        await this.AddPlaylistItems(playlist, playlistItems).ConfigureAwait(false);
                        await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset).ConfigureAwait(false);
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
                await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated, new[] { this.Playlist })).ConfigureAwait(false);
            }

            private async Task AddPlaylistItems(Playlist playlist, IEnumerable<PlaylistItem> playlistItems)
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    var set = this.Database.Set<PlaylistItem>(transaction);
                    var position = 0;
                    foreach (var playlistItem in playlistItems)
                    {
                        Logger.Write(this, LogLevel.Debug, "Adding file to playlist: {0}", playlistItem.FileName);
                        playlistItem.Playlist_Id = playlist.Id;
                        playlistItem.Sequence = this.Sequence + position;
                        playlistItem.Status = PlaylistItemStatus.Import;
                        await set.AddAsync(playlistItem).ConfigureAwait(false);
                        position++;
                    }
                    this.Offset += position;
                    if (transaction.HasTransaction)
                    {
                        transaction.Commit();
                    }
                }
            }
        }
    }
}