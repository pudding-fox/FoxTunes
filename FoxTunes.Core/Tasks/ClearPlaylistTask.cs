using FoxDb;
using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "D2F22C47-386F-4333-AD4F-693951C0E5A1";

        public ClearPlaylistTask()
            : base(ID)
        {

        }

        protected override async Task OnRun()
        {
            this.IsIndeterminate = true;
            Logger.Write(this, LogLevel.Debug, "Clearing playlist.");
            this.Database.Execute(this.Database.Queries.ClearPlaylist);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
