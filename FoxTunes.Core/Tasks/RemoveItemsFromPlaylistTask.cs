using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class RemoveItemsFromPlaylistTask : PlaylistTaskBase
    {
        public RemoveItemsFromPlaylistTask(IEnumerable<PlaylistItem> playlistItems)
            : base()
        {
            this.PlaylistItems = playlistItems;
        }

        public IEnumerable<PlaylistItem> PlaylistItems { get; private set; }

        protected override Task OnRun()
        {
            return this.RemoveItems(this.PlaylistItems);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted();
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
