#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class LibraryTaskBase : BackgroundTask
    {
        protected LibraryTaskBase(string id)
            : base(id)
        {
        }

        public ICore Core { get; private set; }

        public IDatabaseComponent Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Database = core.Components.Database.New();
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected virtual Task SetLibraryItemsStatus(ITransactionSource transaction)
        {
            this.IsIndeterminate = true;
            var query = this.Database.QueryFactory.Build();
            query.Update.SetTable(this.Database.Tables.LibraryItem);
            query.Update.AddColumn(this.Database.Tables.LibraryItem.Column("Status"));
            return this.Database.ExecuteAsync(query, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["status"] = LibraryItemStatus.None;
                        break;
                }
            }, transaction);
        }

        protected virtual Task UpdateVariousArtists(ITransactionSource transaction)
        {
            return this.Database.ExecuteAsync(this.Database.Queries.UpdateLibraryVariousArtists, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["name"] = CustomMetaData.VariousArtists;
                        parameters["type"] = MetaDataItemType.Tag;
                        parameters["numericValue"] = 1;
                        parameters["status"] = LibraryItemStatus.Import;
                        break;
                }
            }, transaction);
        }

        protected override void OnDisposing()
        {
            if (!object.ReferenceEquals(this.Core.Components.Database, this.Database))
            {
                this.Database.Dispose();
            }
            base.OnDisposing();
        }
    }
}
