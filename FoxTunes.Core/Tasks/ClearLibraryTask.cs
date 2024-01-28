using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearLibraryTask : LibraryTaskBase
    {
        public ClearLibraryTask(LibraryItemStatus? status)
            : base()
        {
            this.Status = status;
        }

        public LibraryItemStatus? Status { get; private set; }

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

        protected override async Task OnRun()
        {
            await this.RemoveItems(this.Status);
            if (!this.Status.HasValue)
            {
                await this.ClearRoots();
            }
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted();
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
