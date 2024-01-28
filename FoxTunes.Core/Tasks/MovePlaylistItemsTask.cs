using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class MovePlaylistItemsTask : PlaylistTaskBase
    {
        public MovePlaylistItemsTask(int sequence, IEnumerable<PlaylistItem> playlistItems)
            : base(sequence)
        {
            this.PlaylistItems = playlistItems;
        }

        public IEnumerable<PlaylistItem> PlaylistItems { get; private set; }

        protected override Task OnRun()
        {
            return this.MoveItems(this.PlaylistItems);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted();
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
