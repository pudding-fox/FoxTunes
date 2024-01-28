﻿using FoxDb;
using FoxDb.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class MetaDataWriter : Disposable
    {
        public MetaDataWriter(IDatabase database, IDatabaseQuery query, ITransactionSource transaction)
        {
            this.Command = CreateCommand(database, query, transaction);
        }

        public IDatabaseCommand Command { get; private set; }

        public Task Write(int itemId, MetaDataItem metaDataItem)
        {
            this.Command.Parameters["itemId"] = itemId;
            this.Command.Parameters["name"] = metaDataItem.Name;
            this.Command.Parameters["type"] = metaDataItem.Type;
            this.Command.Parameters["numericValue"] = metaDataItem.NumericValue;
            this.Command.Parameters["textValue"] = metaDataItem.TextValue;
            this.Command.Parameters["fileValue"] = metaDataItem.FileValue;
            return this.Command.ExecuteNonQueryAsync();
        }

        protected override void OnDisposing()
        {
            this.Command.Dispose();
            base.OnDisposing();
        }

        private static IDatabaseCommand CreateCommand(IDatabase database, IDatabaseQuery query, ITransactionSource transaction)
        {
            return database.CreateCommand(query, DatabaseCommandFlags.NoCache, transaction);
        }
    }
}