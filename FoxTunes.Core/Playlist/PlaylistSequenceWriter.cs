using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PlaylistSequenceWriter : Disposable
    {
        public PlaylistSequenceWriter(IDatabaseComponent database, ITransactionSource transaction)
        {
            this.Command = CreateCommand(database, transaction);
        }

        public IDatabaseCommand Command { get; private set; }

        public Task Write(IDatabaseReaderRecord record, object[] values)
        {
            this.Command.Parameters["playlistItemId"] = record["PlaylistItem_Id"];

            for (var a = 0; a < 9; a++)
            {
                var value = default(object);
                if (a < values.Length)
                {
                    value = values[a];
                }
                else
                {
                    value = DBNull.Value;
                }
                this.Command.Parameters[string.Format("value{0}", a + 1)] = value;
            }

            return this.Command.ExecuteNonQueryAsync();
        }

        protected override void OnDisposing()
        {
            this.Command.Dispose();
            base.OnDisposing();
        }

        private static IDatabaseCommand CreateCommand(IDatabaseComponent database, ITransactionSource transaction)
        {
            return database.CreateCommand(
                database.Queries.AddPlaylistSequenceRecord,
                DatabaseCommandFlags.NoCache,
                transaction
            );
        }
    }
}
