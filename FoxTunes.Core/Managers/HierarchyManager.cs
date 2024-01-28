using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes.Managers
{
    public class HierarchyManager : StandardManager, IHierarchyManager
    {
        public ICore Core { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            base.InitializeComponent(core);
        }

        public async Task BuildHierarchies()
        {
            using (var task = new BuildLibraryHierarchiesTask())
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run();
            }
        }

        protected virtual void OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
                return;
            }
            this.BackgroundTask(this, new BackgroundTaskEventArgs(backgroundTask));
        }

        public event BackgroundTaskEventHandler BackgroundTask = delegate { };
    }
}
