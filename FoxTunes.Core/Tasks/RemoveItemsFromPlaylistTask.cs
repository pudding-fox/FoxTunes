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
            await this.RemoveItems(this.PlaylistItems);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
