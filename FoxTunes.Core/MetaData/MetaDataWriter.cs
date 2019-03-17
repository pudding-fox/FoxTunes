using FoxDb;
using FoxDb.Interfaces;
using System;
using System.Collections.Generic;
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

        public async Task Write(int itemId, IEnumerable<MetaDataItem> metaDataItems)
        {
            foreach (var metaDataItem in metaDataItems)
            {
                await this.Write(itemId, metaDataItem);
            }
        }

        public Task Write(int itemId, MetaDataItem metaDataItem)
        {
            if (!this.HasValue(metaDataItem.Value))
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            this.Command.Parameters["itemId"] = itemId;
            this.Command.Parameters["name"] = metaDataItem.Name;
            this.Command.Parameters["type"] = metaDataItem.Type;
            this.Command.Parameters["value"] = metaDataItem.Value;
            return this.Command.ExecuteNonQueryAsync();
        }

        private bool HasValue(string value)
        {
            return !string.IsNullOrEmpty(value) && !string.Equals(value, 0.ToString(), StringComparison.OrdinalIgnoreCase);
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
