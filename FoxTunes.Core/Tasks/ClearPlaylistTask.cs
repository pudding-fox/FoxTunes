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
            var query = this.Database.QueryFactory.Build();
            query.Delete.Touch();
            query.Source.AddTable(this.Database.Tables.PlaylistItem);
            using (var transaction = this.Database.BeginTransaction())
            {
                this.Database.Execute(query, transaction);
                this.CleanupMetaData(transaction);
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
