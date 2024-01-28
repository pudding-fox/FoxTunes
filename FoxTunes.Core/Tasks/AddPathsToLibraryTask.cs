using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToLibraryTask : LibraryTaskBase
    {
        public AddPathsToLibraryTask(IEnumerable<string> paths)
            : base()
        {
            this.Paths = paths;
        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        public IEnumerable<string> Paths { get; private set; }

        protected override Task OnStarted()
        {
            this.Name = "Getting file list";
            this.IsIndeterminate = true;
            return base.OnStarted();
        }

        protected override Task OnRun()
        {
            return this.AddPaths(this.Paths);
        }

        protected override async Task OnCompleted()
        {
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
            await base.OnCompleted();
        }
    }
}
