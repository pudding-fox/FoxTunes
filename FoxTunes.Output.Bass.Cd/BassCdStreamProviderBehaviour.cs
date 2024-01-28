﻿using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Cd;
using ManagedBass.Gapless.Cd;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("C051C82C-3391-4DDC-B856-C4BDEA86ADDC", null, priority: ComponentAttribute.PRIORITY_LOW)]
    public class BassCdStreamProviderBehaviour : StandardBehaviour, IConfigurableComponent, IBackgroundTaskSource, IInvocableComponent
    {
        public const string OPEN_CD = "FFFF";

        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IBassOutput Output { get; private set; }

        public bool IsInitialized { get; private set; }

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
                this.Output.Shutdown();
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
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassCdStreamProviderBehaviourConfiguration.CD_SECTION,
                BassCdStreamProviderBehaviourConfiguration.DRIVE_ELEMENT
            ).ConnectValue<string>(value => this.CdDrive = BassCdStreamProviderBehaviourConfiguration.GetDrive(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassCdStreamProviderBehaviourConfiguration.CD_SECTION,
                BassCdStreamProviderBehaviourConfiguration.LOOKUP_ELEMENT
            ).ConnectValue<bool>(value => this.CdLookup = value);
            this.Configuration.GetElement<TextConfigurationElement>(
                BassCdStreamProviderBehaviourConfiguration.CD_SECTION,
                BassCdStreamProviderBehaviourConfiguration.LOOKUP_HOST_ELEMENT
            ).ConnectValue<string>(value => this.CdLookupHost = value);
            ComponentRegistry.Instance.GetComponent<IBassStreamFactory>().Register(new BassCdStreamProvider());
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
            return Task.CompletedTask;
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

        public event BackgroundTaskEventHandler BackgroundTask = delegate { };

        private class AddCdToPlaylistTask : PlaylistTaskBase
        {
            public const string ID = "8A0B7E8B-B601-4803-9163-B4D42FB0C304";

            public AddCdToPlaylistTask(int drive, bool cdLookup, string cdLookupHost)
                : base(ID)
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
                this.Name = "Opening CD";
                this.IsIndeterminate = true;
                try
                {
                    if (!BassCd.IsReady(this.Drive))
                    {
                        throw new InvalidOperationException("Drive is not ready.");
                    }
                    //Always append for now.
                    this.Sequence = await this.PlaylistManager.GetInsertIndex();
                    using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                    {
                        await this.AddPlaylistItems(transaction);
                        await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset, transaction);
                        await this.AddOrUpdateMetaData(transaction);
                        await this.SetPlaylistItemsStatus(transaction);
                        transaction.Commit();
                    }
                }
                finally
                {
                    //Ignoring result on purpose.
                    BassCd.Release(this.Drive);
                }
                await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
            }

            private async Task AddPlaylistItems(ITransactionSource transaction)
            {
                this.Name = "Getting track list";
                this.IsIndeterminate = true;
                var info = default(CDInfo);
                BassUtils.OK(BassCd.GetInfo(this.Drive, out info));
                var directoryName = string.Format("{0}:\\", info.DriveLetter);
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
            }

            private async Task AddOrUpdateMetaData(ITransactionSource transaction)
            {
                Logger.Write(this, LogLevel.Debug, "Fetching meta data for new playlist items.");
                var query = this.Database
                    .AsQueryable<PlaylistItem>(this.Database.Source(new DatabaseQueryComposer<PlaylistItem>(this.Database), transaction))
                    .Where(playlistItem => playlistItem.Status == PlaylistItemStatus.Import);
                var info = default(CDInfo);
                var strategy = this.GetStrategy();
                var metaDataSource = new BassCdMetaDataSource(strategy);
                BassUtils.OK(BassCd.GetInfo(this.Drive, out info));
                using (var writer = new MetaDataWriter(this.Database, this.Database.Queries.AddPlaylistMetaDataItems, transaction))
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