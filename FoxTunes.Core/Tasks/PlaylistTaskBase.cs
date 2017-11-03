using FoxTunes.Interfaces;
using System.Data;

namespace FoxTunes
{
    public abstract class PlaylistTaskBase : BackgroundTask
    {
        protected PlaylistTaskBase(string id, int sequence = 0)
            : base(id)
        {
            this.Sequence = sequence;
        }

        public int Sequence { get; private set; }

        public int Offset { get; protected set; }

        public ICore Core { get; private set; }

        public IDataManager DataManager { get; private set; }

        public IDatabase Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.DataManager = core.Managers.Data;
            this.Database = core.Components.Database;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        protected virtual void ShiftItems(IDatabaseContext context, IDbTransaction transaction)
        {
            this.IsIndeterminate = true;
            var parameters = default(IDbParameterCollection);
            using (var command = context.Connection.CreateCommand(this.Database.CoreSQL.ShiftPlaylistItems, new[] { "status", "sequence", "offset" }, out parameters))
            {
                command.Transaction = transaction;
                parameters["status"] = PlaylistItemStatus.None;
                parameters["sequence"] = this.Sequence;
                parameters["offset"] = this.Offset;
                command.ExecuteNonQuery();
            }
        }

        protected virtual void SetPlaylistItemsStatus(IDatabaseContext databaseContext, IDbTransaction transaction)
        {
            this.IsIndeterminate = true;
            var parameters = default(IDbParameterCollection);
            using (var command = databaseContext.Connection.CreateCommand(this.Database.CoreSQL.SetPlaylistItemStatus, new[] { "status" }, out parameters))
            {
                command.Transaction = transaction;
                parameters["status"] = LibraryItemStatus.None;
                command.ExecuteNonQuery();
            }
        }
    }
}
