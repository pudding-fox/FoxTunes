using FoxDb;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class RemoveItemsFromPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "E7778FE8-D73D-4263-8C40-FEF179F6C7F7";

        public RemoveItemsFromPlaylistTask(IEnumerable<PlaylistItem> playlistItems)
            : base(ID)
        {
            this.PlaylistItems = playlistItems;
        }

        public IEnumerable<PlaylistItem> PlaylistItems { get; private set; }

        protected override async Task OnRun()
        {
            this.IsIndeterminate = true;
            var query = this.Database.QueryFactory.Delete(this.Database.Tables.PlaylistItem).Build();
            using (var transaction = this.Database.BeginTransaction())
            {
                using (var command = this.Database.CreateCommand(query, transaction))
                {
                    foreach (var playlistItem in this.PlaylistItems)
                    {
                        command.Parameters[Conventions.ParameterName(this.Database.Tables.PlaylistItem.PrimaryKey)] = playlistItem.Id;
                        command.ExecuteNonQuery();
                    }
                }
                this.CleanupMetaData(transaction);
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
