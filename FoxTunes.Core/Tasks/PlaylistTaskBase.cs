using FoxTunes.Interfaces;
using System;
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

        public IDatabase Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Database = core.Components.Database;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        protected virtual void ShiftItems(IDbTransaction transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Shifting playlist items at position {0} by offset {1}.", this.Sequence, this.Offset);
            this.IsIndeterminate = true;
            var parameters = default(IDbParameterCollection);
            using (var command = this.Database.CreateCommand(this.Database.Queries.ShiftPlaylistItems, out parameters))
            {
                command.Transaction = transaction;
                parameters["status"] = PlaylistItemStatus.None;
                parameters["sequence"] = this.Sequence;
                parameters["offset"] = this.Offset;
                command.ExecuteNonQuery();
            }
        }

        protected virtual void SequenceItems(IDbTransaction transaction)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = this.Database.CreateCommand(this.Database.Queries.PlaylistSequenceBuilder(MetaDataInfo.GetMetaDataNames(this.Database, transaction)), out parameters))
            {
                command.Transaction = transaction;
                parameters["status"] = PlaylistItemStatus.Import;
                using (var reader = EnumerableDataReader.Create(command.ExecuteReader()))
                {
                    this.SequenceItems(transaction, reader);
                }
            }
        }

        protected virtual void SequenceItems(IDbTransaction transaction, EnumerableDataReader reader)
        {
            using (var playlistSequencePopulator = new PlaylistSequencePopulator(this.Database, transaction, false))
            {
                playlistSequencePopulator.InitializeComponent(this.Core);
                playlistSequencePopulator.Populate(reader);
            }
        }

        protected virtual void SetPlaylistItemsStatus(IDbTransaction transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Setting playlist status: {0}", Enum.GetName(typeof(LibraryItemStatus), LibraryItemStatus.None));
            this.IsIndeterminate = true;
            var parameters = default(IDbParameterCollection);
            using (var command = this.Database.CreateCommand(this.Database.Queries.SetPlaylistItemStatus, out parameters))
            {
                command.Transaction = transaction;
                parameters["status"] = LibraryItemStatus.None;
                command.ExecuteNonQuery();
            }
        }

        protected virtual void AddOrUpdateMetaDataFromLibrary(IDbTransaction transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Updating playlist items with meta data from library.");
            var parameters = default(IDbParameterCollection);
            using (var command = this.Database.CreateCommand(this.Database.Queries.CopyMetaDataItems, out parameters))
            {
                command.Transaction = transaction;
                parameters["status"] = PlaylistItemStatus.Import;
                command.ExecuteNonQuery();
            }
        }
    }
}
