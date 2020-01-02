using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearLibraryHierarchiesTask : LibraryTaskBase
    {
        public ClearLibraryHierarchiesTask(LibraryItemStatus? status)
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
            await this.SetName("Clearing hierarchies").ConfigureAwait(false);
            await this.SetIsIndeterminate(true).ConfigureAwait(false);
            await base.OnStarted().ConfigureAwait(false);
        }

        protected override Task OnRun()
        {
            return this.RemoveHierarchies(this.Status);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated)).ConfigureAwait(false);
        }
    }
}
