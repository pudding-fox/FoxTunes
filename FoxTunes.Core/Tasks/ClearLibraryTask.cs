using FoxTunes.Interfaces;
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

        protected override Task OnRun()
        {
            this.IsIndeterminate = true;
            Logger.Write(this, LogLevel.Debug, "Clearing library.");
            this.Database.Execute(this.Database.Queries.ClearLibrary);
            this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
            return Task.CompletedTask;
        }
    }
}
