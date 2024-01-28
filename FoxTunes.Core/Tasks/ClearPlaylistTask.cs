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
            using (var transaction = this.Database.BeginTransaction())
            {
                this.ClearItems(transaction);
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
