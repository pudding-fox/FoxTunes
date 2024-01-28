using System.Threading.Tasks;

namespace FoxTunes
{
    public class RemovePlaylistTask : PlaylistTaskBase
    {
        public RemovePlaylistTask(Playlist playlist) : base(playlist)
        {

        }

        protected override Task OnRun()
        {
            return this.RemovePlaylist();
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistsUpdated)).ConfigureAwait(false);
        }
    }
}
