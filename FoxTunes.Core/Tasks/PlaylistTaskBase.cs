using FoxTunes.Interfaces;
using FoxTunes.Tasks;
using System.Data;

namespace FoxTunes
{
    public abstract class PlaylistTaskBase : BackgroundTask
    {
        protected PlaylistTaskBase(string id)
            : base(id)
        {
        }

        public IDataManager DataManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.DataManager = core.Managers.Data;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected virtual void ShiftItems(IDatabaseContext context, IDbTransaction transaction, int sequence, int offset)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = context.Connection.CreateCommand(Resources.ShiftPlaylistItems, new[] { "status", "sequence", "offset" }, out parameters))
            {
                command.Transaction = transaction;
                parameters["status"] = PlaylistItemStatus.None;
                parameters["sequence"] = sequence;
                parameters["offset"] = offset;
                command.ExecuteNonQuery();
            }
        }

        protected virtual void SetPlaylistItemsStatus(IDatabaseContext databaseContext, IDbTransaction transaction)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(Resources.SetPlaylistItemStatus, new[] { "status" }, out parameters))
            {
                command.Transaction = transaction;
                parameters["status"] = LibraryItemStatus.None;
                command.ExecuteNonQuery();
            }
        }
    }
}
