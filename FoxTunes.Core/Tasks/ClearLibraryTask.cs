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

        protected override async Task OnStarted()
        {
            await this.SetName("Clearing library");
            await this.SetIsIndeterminate(true);
            await base.OnStarted();
        }

        protected override Task OnRun()
        {
            return this.RemoveItems(LibraryItemStatus.None);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted();
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
        }
    }
}
