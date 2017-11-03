using FoxTunes.Interfaces;
using FoxTunes.Utilities.Templates;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "7B564369-A6A0-4BAF-8C99-08AF27908591";

        public AddPathsToPlaylistTask(int sequence, IEnumerable<string> paths)
            : base(ID, sequence)
        {
            this.Paths = paths;
        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        public IEnumerable<string> Paths { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.MetaDataSourceFactory = core.Factories.MetaDataSource;
            base.InitializeComponent(core);
        }

        protected override Task OnRun()
        {
            using (var databaseContext = this.DataManager.CreateWriteContext())
            {
                using (var transaction = databaseContext.Connection.BeginTransaction())
                {
                    this.AddPlaylistItems(databaseContext, transaction);
                    this.ShiftItems(databaseContext, transaction);
                    this.AddOrUpdateMetaData(databaseContext, transaction);
                    this.SequenceItems(databaseContext, transaction);
                    this.SetPlaylistItemsStatus(databaseContext, transaction);
                    transaction.Commit();
                }
            }
            this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
            return Task.CompletedTask;
        }

        private void AddPlaylistItems(IDatabaseContext databaseContext, IDbTransaction transaction)
        {
            this.Name = "Getting file list";
            this.IsIndeterminate = true;
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(this.Database.CoreSQL.AddPlaylistItem, new[] { "sequence", "directoryName", "fileName", "status" }, out parameters))
            {
                command.Transaction = transaction;
                var count = 1;
                var addPlaylistItem = new Action<string>(fileName =>
                {
                    if (!this.PlaybackManager.IsSupported(fileName))
                    {
                        return;
                    }
                    parameters["directoryName"] = Path.GetDirectoryName(fileName);
                    parameters["fileName"] = fileName;
                    parameters["sequence"] = this.Sequence;
                    parameters["status"] = PlaylistItemStatus.Import;
                    command.ExecuteNonQuery();
                    count++;
                });
                foreach (var path in this.Paths)
                {
                    if (Directory.Exists(path))
                    {
                        foreach (var fileName in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                        {
                            Logger.Write(this, LogLevel.Debug, "Adding file to playlist: {0}", fileName);
                            addPlaylistItem(fileName);
                        }
                    }
                    else if (File.Exists(path))
                    {
                        Logger.Write(this, LogLevel.Debug, "Adding file to playlist: {0}", path);
                        addPlaylistItem(path);
                    }
                }
                this.Offset = count;
            }
        }

        private void AddOrUpdateMetaData(IDatabaseContext databaseContext, IDbTransaction transaction)
        {
            using (var metaDataPopulator = new MetaDataPopulator(this.Database, databaseContext, transaction, "Playlist"))
            {
                var query = databaseContext.GetQuery<PlaylistItem>().Detach().Where(playlistItem => playlistItem.Status == PlaylistItemStatus.Import);
                metaDataPopulator.InitializeComponent(this.Core);
                metaDataPopulator.NameChanged += (sender, e) => this.Name = metaDataPopulator.Name;
                metaDataPopulator.DescriptionChanged += (sender, e) => this.Description = metaDataPopulator.Description;
                metaDataPopulator.PositionChanged += (sender, e) => this.Position = metaDataPopulator.Position;
                metaDataPopulator.CountChanged += (sender, e) => this.Count = metaDataPopulator.Count;
                metaDataPopulator.Populate(query);
            }
        }

        protected virtual void SequenceItems(IDatabaseContext databaseContext, IDbTransaction transaction)
        {
            var metaDataNames =
                from metaDataItem in databaseContext.GetQuery<MetaDataItem>().Detach()
                group metaDataItem by metaDataItem.Name into name
                select name.Key;
            var libraryHierarchyBuilder = new PlaylistSequenceBuilder(metaDataNames);
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(libraryHierarchyBuilder.TransformText(), new[] { "status" }, out parameters))
            {
                command.Transaction = transaction;
                parameters["status"] = PlaylistItemStatus.Import;
                using (var reader = EnumerableDataReader.Create(command.ExecuteReader()))
                {
                    this.SequenceItems(databaseContext, transaction, reader);
                }
            }
        }

        protected virtual void SequenceItems(IDatabaseContext databaseContext, IDbTransaction transaction, EnumerableDataReader reader)
        {
            using (var playlistSequencePopulator = new PlaylistSequencePopulator(this.Database, databaseContext, transaction))
            {
                playlistSequencePopulator.InitializeComponent(this.Core);
                playlistSequencePopulator.NameChanged += (sender, e) => this.Name = playlistSequencePopulator.Name;
                playlistSequencePopulator.DescriptionChanged += (sender, e) => this.Description = playlistSequencePopulator.Description;
                playlistSequencePopulator.PositionChanged += (sender, e) => this.Position = playlistSequencePopulator.Position;
                playlistSequencePopulator.CountChanged += (sender, e) => this.Count = playlistSequencePopulator.Count;
                playlistSequencePopulator.Populate(reader);
            }
        }
    }
}
