using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class SynchronizePlaylistMetaDataTask : BackgroundTask
    {
        public const string ID = "7B72BD8B-3C09-49B3-AA5D-D650F8BCBF8A";

        public SynchronizePlaylistMetaDataTask(IEnumerable<PlaylistItem> playlistItems, IEnumerable<string> names) : base(ID)
        {
            this.PlaylistItems = playlistItems;
            this.Names = names;
            this.Errors = new Dictionary<PlaylistItem, IList<string>>();
        }

        public IEnumerable<PlaylistItem> PlaylistItems { get; private set; }

        public IEnumerable<string> Names { get; private set; }

        public IDictionary<PlaylistItem, IList<string>> Errors { get; private set; }

        public IOutput Output { get; private set; }

        public IDatabaseComponent Database { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Write { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.Database = core.Factories.Database.Create();
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            this.Configuration = core.Components.Configuration;
            this.Write = this.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.WRITE_ELEMENT
            );
            base.InitializeComponent(core);
        }


        protected override async Task OnRun()
        {
            if (MetaDataBehaviourConfiguration.GetWriteBehaviour(this.Write.Value) == WriteBehaviour.None)
            {
                Logger.Write(this, LogLevel.Debug, "Meta data writing is disabled.");
                return;
            }
            foreach (var playlistItem in this.PlaylistItems)
            {
                if (this.Output.IsLoaded(playlistItem.FileName))
                {
                    Logger.Write(this, LogLevel.Debug, "File \"{0}\" could not be written, the update will be retried: The file is in use.", playlistItem.FileName);
                    this.AddError(playlistItem, string.Format("File \"{0}\" could not be written, the update will be retried: The file is in use.", playlistItem.FileName));
                    await this.Schedule(playlistItem).ConfigureAwait(false);
                    continue;
                }
                try
                {
                    await this.Synchronize(playlistItem).ConfigureAwait(false);
                }
                catch (IOException e)
                {
                    Logger.Write(this, LogLevel.Debug, "File \"{0}\" could not be written, the update will be retried: {1}", playlistItem.FileName, e.Message);
                    this.AddError(playlistItem, string.Format("File \"{0}\" could not be written, the update will be retried: {1}", playlistItem.FileName, e.Message));
                    await this.Schedule(playlistItem).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Debug, "File \"{0}\" could not be written: {1}", playlistItem.FileName, e.Message);
                    this.AddError(playlistItem, string.Format("File \"{0}\" could not be written: {1}", playlistItem.FileName, e.Message));
                }
            }
        }

        protected virtual async Task Synchronize(PlaylistItem playlistItem)
        {
            if (!FileSystemHelper.IsLocalPath(playlistItem.FileName))
            {
                Logger.Write(this, LogLevel.Debug, "File \"{0}\" is not a local file: Cannot update.", playlistItem.FileName);
                return;
            }

            if (!File.Exists(playlistItem.FileName))
            {
                Logger.Write(this, LogLevel.Debug, "File \"{0}\" no longer exists: Cannot update.", playlistItem.FileName);
                return;
            }

            var metaDataSource = this.MetaDataSourceFactory.Create();
            await metaDataSource.SetMetaData(
                playlistItem.FileName,
                playlistItem.MetaDatas,
                metaDataItem => this.Names == null || !this.Names.Any() || this.Names.Contains(metaDataItem.Name, true)
            ).ConfigureAwait(false);

            //Update the import date otherwise the file might be re-scanned and changes lost.
            if (playlistItem.LibraryItem_Id.HasValue)
            {
                var libraryItem = new LibraryItem()
                {
                    Id = playlistItem.LibraryItem_Id.Value
                };
                await LibraryTaskBase.SetLibraryItemImportDate(this.Database, libraryItem, DateTime.UtcNow).ConfigureAwait(false);
            }

            await PlaylistTaskBase.SetPlaylistItemStatus(this.Database, playlistItem.Id, PlaylistItemStatus.None).ConfigureAwait(false);
        }

        protected virtual Task Schedule(PlaylistItem playlistItem)
        {
            if (playlistItem.LibraryItem_Id.HasValue)
            {
                return LibraryTaskBase.SetLibraryItemStatus(this.Database, playlistItem.LibraryItem_Id.Value, LibraryItemStatus.Export);
            }
            return PlaylistTaskBase.SetPlaylistItemStatus(this.Database, playlistItem.Id, PlaylistItemStatus.Export);
        }

        protected virtual void AddError(PlaylistItem playlistItem, string message)
        {
            var errors = default(IList<string>);
            if (!this.Errors.TryGetValue(playlistItem, out errors))
            {
                errors = new List<string>();
                this.Errors.Add(playlistItem, errors);
            }
            errors.Add(message);
        }
    }
}
