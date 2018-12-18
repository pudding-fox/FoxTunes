#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearLibraryTask : LibraryTaskBase
    {
        public const string ID = "08D1EDBA-B9FF-4327-88BE-0CDB6DD4531C";

        public ClearLibraryTask()
            : base(ID)
        {

        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        protected override Task OnStarted()
        {
            this.Name = "Clearing library";
            this.IsIndeterminate = true;
            return base.OnStarted();
        }

        protected override async Task OnRun()
        {
            Logger.Write(this, LogLevel.Debug, "Clearing library.");
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                await this.RemoveItems(LibraryItemStatus.None, transaction);
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
        }
    }
}
