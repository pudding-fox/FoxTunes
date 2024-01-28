using System.Threading.Tasks;

namespace FoxTunes
{
    public class BuildLibraryHierarchiesTask : LibraryTaskBase
    {
        public BuildLibraryHierarchiesTask()
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
            this.Name = "Building hierarchies";
            this.Description = "Preparing";
            this.IsIndeterminate = true;
            return base.OnStarted();
        }

        protected override async Task OnRun()
        {
            await this.BuildHierarchies();
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
        }
    }
}
