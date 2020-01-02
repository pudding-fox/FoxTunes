using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearPlaylistTask : PlaylistTaskBase
    {
        public ClearPlaylistTask()
            : base()
        {

        }

        protected override Task OnRun()
        {
            return this.RemoveItems(PlaylistItemStatus.None);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated)).ConfigureAwait(false);
        }
    }
}
