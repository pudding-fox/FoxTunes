using FoxTunes.Interfaces;
using FoxTunes.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(Resources.AddPlaylistItem, new[] { "sequence", "directoryName", "fileName", "status" }, out parameters))
            {
                var sequence = 0;
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
                foreach (var libraryItem in this.LibraryItems)
                {
                    addPlaylistItem(libraryItem.FileName);
                }
                this.Offset = sequence;
            }
        }

        private void AddOrUpdateMetaData(IDatabaseContext databaseContext)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(Resources.CopyMetaDataItems, new[] { "status" }, out parameters))
            {
                parameters["status"] = PlaylistItemStatus.Import;
                command.ExecuteNonQuery();
            }
        }
    }
}
