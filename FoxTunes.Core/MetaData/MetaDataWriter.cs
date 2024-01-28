using FoxDb;
using FoxDb.Interfaces;

namespace FoxTunes
{
    public class MetaDataWriter : Disposable
    {
        public MetaDataWriter(IDatabase database, ITransactionSource transaction, IDatabaseQuery query)
        {
            this.Command = CreateCommand(database, transaction, query);
        }

        public IDatabaseCommand Command { get; private set; }

        public void Write(int itemId, MetaDataItem metaDataItem)
        {
            this.Command.Parameters["itemId"] = itemId;
            this.Command.Parameters["name"] = metaDataItem.Name;
            this.Command.Parameters["type"] = metaDataItem.Type;
            this.Command.Parameters["numericValue"] = metaDataItem.NumericValue;
            this.Command.Parameters["textValue"] = metaDataItem.TextValue;
            this.Command.Parameters["fileValue"] = metaDataItem.FileValue;
            this.Command.ExecuteNonQuery();
        }

        protected override void OnDisposing()
        {
            this.Command.Dispose();
            base.OnDisposing();
        }

        private static IDatabaseCommand CreateCommand(IDatabase database, ITransactionSource transaction, IDatabaseQuery query)
        {
            return database.CreateCommand(query, DatabaseCommandFlags.NoCache, transaction);
        }
    }
}
