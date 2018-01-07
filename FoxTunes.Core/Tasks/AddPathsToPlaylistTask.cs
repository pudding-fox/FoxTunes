using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
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
            using (ITransactionSource transaction = this.Database.BeginTransaction())
            {
                this.AddPlaylistItems(transaction);
                this.ShiftItems(transaction);
                this.AddOrUpdateMetaDataFromLibrary(transaction);
                this.AddOrUpdateMetaData(transaction);
                this.SequenceItems(transaction);
                this.SetPlaylistItemsStatus(transaction);
                transaction.Commit();
            }
            this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
            return Task.CompletedTask;
        }

        private void AddPlaylistItems(ITransactionSource transaction)
        {
            this.Name = "Getting file list";
            this.IsIndeterminate = true;
            var parameters = default(IDatabaseParameters);
            using (var command = this.Database.Connection.CreateCommand(this.Database.Queries.AddPlaylistItem, out parameters))
            {
                transaction.Bind(command);
                var count = 0;
                var addPlaylistItem = new Action<string>(fileName =>
                {
                    if (!this.PlaybackManager.IsSupported(fileName))
                    {
                        Logger.Write(this, LogLevel.Debug, "File is not supported: {0}", fileName);
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

        private void AddOrUpdateMetaData(ITransactionSource transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Fetching meta data for new playlist items.");
            using (var metaDataPopulator = new MetaDataPopulator(this.Database, transaction, this.Database.Queries.AddPlaylistMetaDataItems, true))
            {
                var enumerable = this.Database.ExecuteEnumerator<PlaylistItem>(this.Database.Queries.GetPlaylistItemsWithoutMetaData, parameters => parameters["status"] = PlaylistItemStatus.Import, transaction);
                metaDataPopulator.InitializeComponent(this.Core);
                metaDataPopulator.NameChanged += (sender, e) => this.Name = metaDataPopulator.Name;
                metaDataPopulator.DescriptionChanged += (sender, e) => this.Description = metaDataPopulator.Description;
                metaDataPopulator.PositionChanged += (sender, e) => this.Position = metaDataPopulator.Position;
                metaDataPopulator.CountChanged += (sender, e) => this.Count = metaDataPopulator.Count;
                metaDataPopulator.Populate(enumerable);
            }
        }
    }
}
