using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes.Managers
{
    public class HierarchyManager : StandardManager, IHierarchyManager
    {
        public ICore Core { get; private set; }

        public ILibrary Library { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Library = core.Components.Library;
            base.InitializeComponent(core);
        }

        public Task AddHierarchy(LibraryHierarchy libraryHierarchy)
        {
            this.Library.LibraryHierarchySet.Add(libraryHierarchy);
            return Task.CompletedTask;
        }

        public Task DeleteHierarchy(LibraryHierarchy libraryHierarchy)
        {
            this.Library.LibraryHierarchySet.Remove(libraryHierarchy);
            return Task.CompletedTask;
        }

        public Task BuildHierarchies()
        {
            var task = new BuildLibraryHierarchiesTask();
            task.InitializeComponent(this.Core);
            this.OnBackgroundTask(task);
            return task.Run();
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
