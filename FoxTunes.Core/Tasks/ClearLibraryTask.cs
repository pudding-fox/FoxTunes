using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearLibraryTask : LibraryTaskBase
    {
        public ClearLibraryTask()
            : base()
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

        protected override Task OnRun()
        {
            return this.RemoveItems(LibraryItemStatus.None);
        }

        protected override async Task OnCompleted()
        {
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
            await base.OnCompleted();
        }
    }
}
