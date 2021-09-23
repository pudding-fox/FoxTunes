using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.ZipStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("5E1331EE-37F9-41BB-BD5E-82E9B4995B8A", null, priority: ComponentAttribute.PRIORITY_LOW)]
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassArchiveStreamProviderBehaviour : StandardBehaviour, IConfigurableComponent, IBackgroundTaskSource, IInvocableComponent, IFileActionHandler, IDisposable
    {
        public const string OPEN_ARCHIVE = "FGGG";

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IFileSystemBrowser FileSystemBrowser { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IBassOutput Output { get; private set; }

        new public bool IsInitialized { get; private set; }

        private int _BufferMin { get; set; }

        public int BufferMin
        {
            get
            {
                return this._BufferMin;
            }
            set
            {
                this._BufferMin = value;
                BassZipStream.SetConfig(BassZipStreamAttribute.BufferMin, value);
                Logger.Write(this, LogLevel.Debug, "BufferMin = {0}", this.BufferMin);
            }
        }

        private int _BufferTimeout { get; set; }

        public int BufferTimeout
        {
            get
            {
                return this._BufferTimeout;
            }
            set
            {
                this._BufferTimeout = value;
                BassZipStream.SetConfig(BassZipStreamAttribute.BufferTimeout, value);
                Logger.Write(this, LogLevel.Debug, "BufferTimeout = {0}", this.BufferTimeout);
            }
        }

        private bool _DoubleBuffer { get; set; }

        public bool DoubleBuffer
        {
            get
            {
                return this._DoubleBuffer;
            }
            set
            {
                this._DoubleBuffer = value;
                BassZipStream.SetConfig(BassZipStreamAttribute.DoubleBuffer, value);
                Logger.Write(this, LogLevel.Debug, "DoubleBuffer = {0}", this.DoubleBuffer);
            }
        }

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

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            this.PlaylistManager = core.Managers.Playlist;
            this.FileSystemBrowser = core.Components.FileSystemBrowser;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<IntegerConfigurationElement>(
                BassArchiveStreamProviderBehaviourConfiguration.SECTION,
                BassArchiveStreamProviderBehaviourConfiguration.BUFFER_MIN_ELEMENT
            ).ConnectValue(value => this.BufferMin = value);
            this.Configuration.GetElement<IntegerConfigurationElement>(
                BassArchiveStreamProviderBehaviourConfiguration.SECTION,
                BassArchiveStreamProviderBehaviourConfiguration.BUFFER_TIMEOUT_ELEMENT
            ).ConnectValue(value => this.BufferTimeout = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassArchiveStreamProviderBehaviourConfiguration.SECTION,
                BassArchiveStreamProviderBehaviourConfiguration.DOUBLE_BUFFER_ELEMENT
            ).ConnectValue(value => this.DoubleBuffer = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassArchiveStreamProviderBehaviourConfiguration.SECTION,
                BassArchiveStreamProviderBehaviourConfiguration.ENABLED_ELEMENT
            ).ConnectValue(value => this.Enabled = value);
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
            BassUtils.OK(BassZipStream.Init());
            BassUtils.OK(BassZipStream.SetConfig(BassZipStreamAttribute.BufferMin, this.BufferMin));
            BassUtils.OK(BassZipStream.SetConfig(BassZipStreamAttribute.BufferTimeout, this.BufferTimeout));
            BassUtils.OK(BassZipStream.SetConfig(BassZipStreamAttribute.DoubleBuffer, this.DoubleBuffer));
            this.IsInitialized = true;
            Logger.Write(this, LogLevel.Debug, "BASS ZIPSTREAM Initialized.");
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            if (!this.IsInitialized)
            {
                return;
            }
            BassZipStream.Free();
            this.IsInitialized = false;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassArchiveStreamProviderBehaviourConfiguration.GetConfigurationSections();
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, OPEN_ARCHIVE, "Open Archive");
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case OPEN_ARCHIVE:
                    return this.OpenArchive();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public bool CanHandle(string path, FileActionType type)
        {
            if (!this.Enabled)
            {
                return false;
            }
            if (type != FileActionType.Playlist)
            {
                return false;
            }
            if (!File.Exists(path) || !ArchiveUtils.Extensions.Contains(path.GetExtension(), StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public async Task Handle(IEnumerable<string> paths, FileActionType type)
        {
            switch (type)
            {
                case FileActionType.Playlist:
                    var playlist = this.PlaylistManager.CurrentPlaylist ?? this.PlaylistManager.SelectedPlaylist;
                    foreach (var path in paths)
                    {
                        await this.OpenArchive(playlist, path).ConfigureAwait(false);
                    }
                    break;
            }
        }

        public Task OpenArchive()
        {
            var options = new BrowseOptions(
                "Open",
                string.Empty,
                new[]
                {
                    new BrowseFilter("Archives", ArchiveUtils.Extensions)
                },
                BrowseFlags.File
            );
            var result = this.FileSystemBrowser.Browse(options);
            if (!result.Success)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.OpenArchive(this.PlaylistManager.SelectedPlaylist, result.Paths.FirstOrDefault());
        }

        public async Task OpenArchive(Playlist playlist, string fileName)
        {
            using (var task = new AddArchiveToPlaylistTask(playlist, fileName))
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

        ~BassArchiveStreamProviderBehaviour()
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

        private class AddArchiveToPlaylistTask : PlaylistTaskBase
        {
            public AddArchiveToPlaylistTask(Playlist playlist, string fileName) : base(playlist)
            {
                this.FileName = fileName;
            }

            public string FileName { get; private set; }

            public IPlaylistBrowser PlaylistBrowser { get; private set; }

            public ArchivePlaylistItemFactory Factory { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                //Clear any cached passwords if possible.
                var password = ComponentRegistry.Instance.GetComponent<BassArchiveStreamPasswordBehaviour>();
                if (password != null)
                {
                    password.Reset();
                }
                this.PlaylistBrowser = core.Components.PlaylistBrowser;
                this.Factory = new ArchivePlaylistItemFactory(this.FileName);
                this.Factory.InitializeComponent(core);
                base.InitializeComponent(core);
            }

            protected override async Task OnRun()
            {
                this.Name = "Opening archive";
                var playlistItems = await this.Factory.Create().ConfigureAwait(false);
                using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
                {
                    //Always append for now.
                    this.Sequence = this.PlaylistBrowser.GetInsertIndex(this.Playlist);
                    await this.AddPlaylistItems(this.Playlist, playlistItems).ConfigureAwait(false);
                    await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset).ConfigureAwait(false);
                    await this.SetPlaylistItemsStatus(PlaylistItemStatus.None).ConfigureAwait(false);
                }))
                {
                    await task.Run().ConfigureAwait(false);
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