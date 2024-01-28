using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPlaylistTask : PlaylistTaskBase
    {
        public AddPlaylistTask(Playlist playlist) : base(playlist)
        {

        }

        protected override Task OnRun()
        {
            return this.AddPlaylist();
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated, new[] { this.Playlist })).ConfigureAwait(false);
        }
    }
}
