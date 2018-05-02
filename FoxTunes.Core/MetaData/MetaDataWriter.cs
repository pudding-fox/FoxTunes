using FoxDb;
using FoxDb.Interfaces;
using System.Data;

namespace FoxTunes
{
    public class MetaDataWriter : Disposable
    {
        public MetaDataWriter(IDatabase database, ITransactionSource transaction, IDatabaseQuery query)
        {
            var parameters = default(IDatabaseParameters);
            this.Command = CreateCommand(database, transaction, query, out parameters);
            this.Parameters = parameters;
        }

        public IDbCommand Command { get; private set; }

        public IDatabaseParameters Parameters { get; private set; }

        public void Write(int itemId, MetaDataItem metaDataItem)
        {
            this.Parameters["itemId"] = itemId;
            this.Parameters["name"] = metaDataItem.Name;
            this.Parameters["type"] = metaDataItem.Type;
            this.Parameters["numericValue"] = metaDataItem.NumericValue;
            this.Parameters["textValue"] = metaDataItem.TextValue;
            this.Parameters["fileValue"] = metaDataItem.FileValue;
            this.Command.ExecuteNonQuery();
        }

        protected override void OnDisposing()
        {
            this.Command.Dispose();
            base.OnDisposing();
        }

        private static IDbCommand CreateCommand(IDatabase database, ITransactionSource transaction, IDatabaseQuery query, out IDatabaseParameters parameters)
        {
            return database.CreateCommand(query, out parameters, transaction);
        }
    }
}
