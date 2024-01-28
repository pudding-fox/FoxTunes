using FoxTunes.Interfaces;
using FoxTunes.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "7B564369-A6A0-4BAF-8C99-08AF27908591";

        public AddPathsToPlaylistTask(int sequence, IEnumerable<string> paths)
            : base(ID)
        {
            this.Sequence = sequence;
            this.Paths = paths;
        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        public int Sequence { get; private set; }

        public int Offset { get; private set; }

        public IEnumerable<string> Paths { get; private set; }

        public ICore Core { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IMetaDataSourceFactory MetaDataSourceFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
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
                    this.AddPlaylistItems(databaseContext);
                    this.ShiftItems(databaseContext, this.Sequence, this.Offset);
                    this.AddOrUpdateMetaData(databaseContext);
                    this.SetPlaylistItemsStatus(databaseContext);
                    transaction.Commit();
                }
            }
            this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
            return Task.CompletedTask;
        }

        private void AddPlaylistItems(IDatabaseContext databaseContext)
        {
            this.Name = "Getting file list";
            this.IsIndeterminate = true;
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(Resources.AddPlaylistItem, new[] { "sequence", "directoryName", "fileName", "status" }, out parameters))
            {
                var sequence = 1;
                var addPlaylistItem = new Action<string>(fileName =>
                {
                    if (!this.PlaybackManager.IsSupported(fileName))
                    {
                        return;
                    }
                    parameters["sequence"] = this.Sequence + sequence++;
                    parameters["directoryName"] = Path.GetDirectoryName(fileName);
                    parameters["fileName"] = fileName;
                    parameters["status"] = PlaylistItemStatus.Import;
                    command.ExecuteNonQuery();
                });
                foreach (var path in this.Paths)
                {
                    if (Directory.Exists(path))
                    {
                        foreach (var fileName in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                        {
                            addPlaylistItem(fileName);
                        }
                    }
                    else if (File.Exists(path))
                    {
                        addPlaylistItem(path);
                    }
                }
                this.Offset = sequence;
            }
        }

        private void AddOrUpdateMetaData(IDatabaseContext databaseContext)
        {
            using (var metaDataPopulator = new MetaDataPopulator(databaseContext, "Playlist"))
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
    }
}
