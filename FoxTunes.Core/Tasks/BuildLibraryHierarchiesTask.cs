using System.Threading.Tasks;

namespace FoxTunes
{
    public class BuildLibraryHierarchiesTask : LibraryTaskBase
    {
        public BuildLibraryHierarchiesTask(bool reset)
            : base()
        {
            this.Reset = reset;
        }

        public bool Reset { get; private set; }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        protected override async Task OnStarted()
        {
            await this.SetName("Building hierarchies");
            await this.SetDescription("Preparing");
            await this.SetIsIndeterminate(true);
            await base.OnStarted();
        }

        protected override async Task OnRun()
        {
            if (this.Reset)
            {
                await this.RemoveHierarchies();
            }
            await this.BuildHierarchies();
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted();
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
        }
    }
}
