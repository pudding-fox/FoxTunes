using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearPlaylistTask : PlaylistTaskBase
    {
        public ClearPlaylistTask()
            : base()
        {

        }

        protected override async Task OnRun()
        {
            await this.RemoveItems(PlaylistItemStatus.None);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
