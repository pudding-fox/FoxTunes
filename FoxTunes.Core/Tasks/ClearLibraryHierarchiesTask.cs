using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearLibraryHierarchiesTask : LibraryTaskBase
    {
        public ClearLibraryHierarchiesTask()
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
            await this.SetName("Clearing hierarchies");
            await this.SetIsIndeterminate(true);
            await base.OnStarted();
        }

        protected override Task OnRun()
        {
            return this.RemoveHierarchies();
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted();
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
        }
    }
}
