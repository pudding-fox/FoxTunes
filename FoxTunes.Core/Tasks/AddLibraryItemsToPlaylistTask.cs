using FoxTunes.Interfaces;
using FoxTunes.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes
{
    public class AddLibraryItemsToPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "4E0DD392-1138-4DA8-84C2-69B27D1E34EA";

        public AddLibraryItemsToPlaylistTask(int sequence, IEnumerable<LibraryItem> libraryItems) :
            base(ID)
        {
            this.Sequence = sequence;
            this.LibraryItems = libraryItems;
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

        public IEnumerable<LibraryItem> LibraryItems { get; private set; }

        public ICore Core { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        protected override Task OnRun()
        {
            using (var databaseContext = this.DataManager.CreateWriteContext())
            {
                using (var transaction = databaseContext.Connection.BeginTransaction())
                {
                    this.AddPlaylistItems(databaseContext, transaction);
                    this.ShiftItems(databaseContext, transaction, this.Sequence, this.Offset);
                    this.AddOrUpdateMetaData(databaseContext, transaction);
                    this.SetPlaylistItemsStatus(databaseContext, transaction);
                    transaction.Commit();
                }
            }
            this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
            return Task.CompletedTask;
        }

        private void AddPlaylistItems(IDatabaseContext databaseContext, IDbTransaction transaction)
        {
            this.Name = "Processing library items";
            this.Position = 0;
            this.Count = this.LibraryItems.Count();
            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            Logger.Write(this, LogLevel.Debug, "Converting library items to playlist items.");
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(Resources.AddPlaylistItem, new[] { "sequence", "directoryName", "fileName", "status" }, out parameters))
            {
                command.Transaction = transaction;
                var position = 0;
                var addPlaylistItem = new Action<string>(fileName =>
                {
                    if (!this.PlaybackManager.IsSupported(fileName))
                    {
                        return;
                    }
                    parameters["sequence"] = this.Sequence + position++;
                    parameters["directoryName"] = Path.GetDirectoryName(fileName);
                    parameters["fileName"] = fileName;
                    parameters["status"] = PlaylistItemStatus.Import;
                    command.ExecuteNonQuery();
                });
                foreach (var libraryItem in this.LibraryItems)
                {
                    Logger.Write(this, LogLevel.Debug, "Adding item to playlist: {0} => {1}", libraryItem.Id, libraryItem.FileName);
                    addPlaylistItem(libraryItem.FileName);
                    if (position % interval == 0)
                    {
                        this.Description = Path.GetFileName(libraryItem.FileName);
                        this.Position = position;
                    }
                }
                this.Offset = position;
            }
        }

        private void AddOrUpdateMetaData(IDatabaseContext databaseContext, IDbTransaction transaction)
        {
            this.IsIndeterminate = true;
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(Resources.CopyMetaDataItems, new[] { "status" }, out parameters))
            {
                command.Transaction = transaction;
                parameters["status"] = PlaylistItemStatus.Import;
                command.ExecuteNonQuery();
            }
        }
    }
}
