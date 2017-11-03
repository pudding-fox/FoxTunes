using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToLibraryTask : LibraryTaskBase
    {
        public const string ID = "972222C8-8F6E-44CF-8EBE-DA4FCFD7CD80";

        public const int SAVE_INTERVAL = 1000;

        public AddPathsToLibraryTask(IEnumerable<string> paths)
            : base(ID)
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
                    this.AddLibraryItems(databaseContext, transaction);
                    this.AddOrUpdateMetaData(databaseContext, transaction);
                    this.SetLibraryItemsStatus(databaseContext, transaction);
                    transaction.Commit();
                }
            }
            this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
            return Task.CompletedTask;
        }

        private void AddLibraryItems(IDatabaseContext databaseContext, IDbTransaction transaction)
        {
            this.Name = "Getting file list";
            this.IsIndeterminate = true;
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(this.Database.CoreSQL.AddLibraryItem, new[] { "directoryName", "fileName", "status" }, out parameters))
            {
                command.Transaction = transaction;
                var addLibraryItem = new Action<string>(fileName =>
                {
                    if (!this.PlaybackManager.IsSupported(fileName))
                    {
                        return;
                    }
                    parameters["directoryName"] = Path.GetDirectoryName(fileName);
                    parameters["fileName"] = fileName;
                    parameters["status"] = LibraryItemStatus.Import;
                    command.ExecuteNonQuery();
                });
                foreach (var path in this.Paths)
                {
                    if (Directory.Exists(path))
                    {
                        foreach (var fileName in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                        {
                            Logger.Write(this, LogLevel.Debug, "Adding file to library: {0}", fileName);
                            addLibraryItem(fileName);
                        }
                    }
                    else if (File.Exists(path))
                    {
                        Logger.Write(this, LogLevel.Debug, "Adding file to library: {0}", path);
                        addLibraryItem(path);
                    }
                }
            }
        }

        private void AddOrUpdateMetaData(IDatabaseContext databaseContext, IDbTransaction transaction)
        {
            using (var metaDataPopulator = new MetaDataPopulator(this.Database, databaseContext, transaction, "Library", true))
            {
                var query = databaseContext.GetQuery<LibraryItem>().Detach().Where(libraryItem => libraryItem.Status == LibraryItemStatus.Import);
                metaDataPopulator.InitializeComponent(this.Core);
                metaDataPopulator.NameChanged += (sender, e) => this.Name = metaDataPopulator.Name;
                metaDataPopulator.DescriptionChanged += (sender, e) => this.Description = metaDataPopulator.Description;
                metaDataPopulator.PositionChanged += (sender, e) => this.Position = metaDataPopulator.Position;
                metaDataPopulator.CountChanged += (sender, e) => this.Count = metaDataPopulator.Count;
                metaDataPopulator.Populate(query);
            }
        }

        private void SetLibraryItemsStatus(IDatabaseContext databaseContext, IDbTransaction transaction)
        {
            this.IsIndeterminate = true;
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(this.Database.CoreSQL.SetLibraryItemStatus, new[] { "status" }, out parameters))
            {
                command.Transaction = transaction;
                parameters["status"] = LibraryItemStatus.None;
                command.ExecuteNonQuery();
            }
        }
    }
}
