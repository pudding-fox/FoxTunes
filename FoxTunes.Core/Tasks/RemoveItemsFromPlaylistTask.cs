using FoxDb;
using System;
using System.Collections.Generic;
using System.Data;
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
            using (var transaction = this.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                foreach (var playlistItem in this.PlaylistItems)
                {
                    playlistItem.Status = PlaylistItemStatus.Remove;
                }
                var set = this.Database.Set<PlaylistItem>(transaction);
                await set.AddOrUpdateAsync(this.PlaylistItems);
                await this.RemoveItems(PlaylistItemStatus.Remove, transaction);
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
