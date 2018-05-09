using System.Collections.Generic;
using System.Threading.Tasks;
using FoxDb;
using FoxDb.Interfaces;

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
                var parameters = default(IDatabaseParameters);
                using (var command = this.Database.CreateCommand(query, out parameters, transaction))
                {
                    foreach (var playlistItem in this.PlaylistItems)
                    {
                        parameters[Conventions.ParameterName(this.Database.Tables.PlaylistItem.PrimaryKey)] = playlistItem.Id;
                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
