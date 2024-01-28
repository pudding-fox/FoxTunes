using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;

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

        public IDatabaseComponent Database { get; private set; }

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

        protected virtual void ShiftItems(ITransactionSource transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Shifting playlist items at position {0} by offset {1}.", this.Sequence, this.Offset);
            this.IsIndeterminate = true;
            this.Database.Execute(this.Database.Queries.ShiftPlaylistItems, parameters =>
            {
                parameters["status"] = PlaylistItemStatus.None;
                parameters["sequence"] = this.Sequence;
                parameters["offset"] = this.Offset;
            }, transaction);
        }

        protected virtual void SequenceItems(ITransactionSource transaction)
        {
            var metaDataNames = MetaDataInfo.GetMetaDataNames(this.Database, transaction);
            using (var reader = this.Database.ExecuteReader(this.Database.Queries.PlaylistSequenceBuilder(metaDataNames), parameters =>
            {
                parameters["status"] = PlaylistItemStatus.Import;
            }, transaction))
            {
                this.SequenceItems(transaction, reader);
            }
        }

        protected virtual void SequenceItems(ITransactionSource transaction, IDatabaseReader reader)
        {
            using (var playlistSequencePopulator = new PlaylistSequencePopulator(this.Database, transaction, false))
            {
                playlistSequencePopulator.InitializeComponent(this.Core);
                playlistSequencePopulator.Populate(reader);
            }
        }

        protected virtual void SetPlaylistItemsStatus(ITransactionSource transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Setting playlist status: {0}", Enum.GetName(typeof(LibraryItemStatus), LibraryItemStatus.None));
            this.IsIndeterminate = true;
            this.Database.Execute(this.Database.Queries.SetPlaylistItemStatus, parameters => parameters["status"] = LibraryItemStatus.None, transaction);
        }

        protected virtual void AddOrUpdateMetaDataFromLibrary(ITransactionSource transaction)
        {
            Logger.Write(this, LogLevel.Debug, "Updating playlist items with meta data from library.");
            this.Database.Execute(this.Database.Queries.CopyMetaDataItems, parameters => parameters["status"] = LibraryItemStatus.Import, transaction);
        }
    }
}
