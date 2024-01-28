using System.Threading.Tasks;

namespace FoxTunes
{
    public class ClearLibraryHierarchiesTask : LibraryTaskBase
    {
        public ClearLibraryHierarchiesTask()
            : base()
        {
        }

        protected override Task OnRun()
        {
            return this.RemoveHierarchies();
        }

        protected override async Task OnCompleted()
        {
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
            await base.OnCompleted();
        }
    }
}
