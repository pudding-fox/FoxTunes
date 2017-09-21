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

        protected override Task OnRun()
        {
            this.IsIndeterminate = true;
            Logger.Write(this, LogLevel.Debug, "Clearing playlist.");
            using (var context = this.DataManager.CreateWriteContext())
            {
                using (var command = context.Connection.CreateCommand(Resources.ClearPlaylist))
                {
                    command.ExecuteNonQuery();
                }
            }
            this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
            return Task.CompletedTask;
        }
    }
}
