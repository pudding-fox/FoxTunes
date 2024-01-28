using FoxTunes.Interfaces;
using System.Data;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddLibraryHierarchyNodeToPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "4E0DD392-1138-4DA8-84C2-69B27D1E34EA";

        public AddLibraryHierarchyNodeToPlaylistTask(int sequence, LibraryHierarchyNode libraryHierarchyNode) : base(ID)
        {
            this.Sequence = sequence;
            this.LibraryHierarchyNode = libraryHierarchyNode;
        }

        public int Sequence { get; private set; }

        public int Offset { get; private set; }

        public LibraryHierarchyNode LibraryHierarchyNode { get; private set; }

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
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(Resources.AddLibraryHierarchyNodeToPlaylist, new[] { "libraryHierarchyId", "libraryHierarchyLevelId", "displayValue", "sequence", "status" }, out parameters))
            {
                command.Transaction = transaction;
                parameters["libraryHierarchyId"] = this.LibraryHierarchyNode.LibraryHierarchyId;
                parameters["libraryHierarchyLevelId"] = this.LibraryHierarchyNode.LibraryHierarchyLevelId;
                parameters["displayValue"] = this.LibraryHierarchyNode.Value;
                parameters["sequence"] = this.Sequence;
                parameters["status"] = PlaylistItemStatus.Import;
                this.Offset = command.ExecuteNonQuery();
            }
        }

        private void AddOrUpdateMetaData(IDatabaseContext databaseContext, IDbTransaction transaction)
        {
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
