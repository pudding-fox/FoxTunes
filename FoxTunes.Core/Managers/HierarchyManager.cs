using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes
{
    public class HierarchyManager : StandardManager, IHierarchyManager
    {
        public HierarchyManagerState State
        {
            get
            {
                if (global::FoxTunes.BackgroundTask.Active.Any(backgroundTask =>
                {
                    var type = backgroundTask.GetType();
                    return type == typeof(BuildLibraryHierarchiesTask) || type == typeof(ClearLibraryHierarchiesTask);
                }))
                {
                    return HierarchyManagerState.Updating;
                }
                return HierarchyManagerState.None;
            }
        }

        public ICore Core { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.LibraryManager = core.Managers.Library;
            base.InitializeComponent(core);
        }

        public async Task Build(bool reset)
        {
            if (this.LibraryManager.State.HasFlag(LibraryManagerState.Updating))
            {
                return;
            }
            using (var task = new BuildLibraryHierarchiesTask(reset))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task);
                await task.Run();
            }
        }

        public async Task Clear()
        {
            if (this.LibraryManager.State.HasFlag(LibraryManagerState.Updating))
            {
                return;
            }
            using (var task = new ClearLibraryHierarchiesTask())
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task);
                await task.Run();
            }
        }

        protected virtual Task OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new BackgroundTaskEventArgs(backgroundTask);
            this.BackgroundTask(this, e);
            return e.Complete();
        }

        public event BackgroundTaskEventHandler BackgroundTask;

        public static void CreateDefaultData(IDatabase database, ICoreScripts scripts)
        {
            using (var transaction = database.BeginTransaction())
            {
                var set = database.Set<LibraryHierarchy>(transaction);
                set.Clear();
                set.Add(new LibraryHierarchy()
                {
                    Name = "Artist/Album/Title",
                    Sequence = 0,
                    Levels = new ObservableCollection<LibraryHierarchyLevel>()
                    {
                        new LibraryHierarchyLevel() { Name = "Artist", Sequence = 0, Script = scripts.Artist },
                        new LibraryHierarchyLevel() { Name = "Year - Album", Sequence = 1, Script = scripts.Year_Album },
                        new LibraryHierarchyLevel() { Name = "Disk - Track - Title", Sequence = 2, Script = scripts.Disk_Track_Title }
                    }
                });
                set.Add(new LibraryHierarchy()
                {
                    Name = "Genre/Album/Title",
                    Sequence = 1,
                    Levels = new ObservableCollection<LibraryHierarchyLevel>()
                    {
                        new LibraryHierarchyLevel() { Name = "Genre", Sequence = 0, Script = scripts.Genre },
                        new LibraryHierarchyLevel() { Name = "Year - Album", Sequence = 1, Script = scripts.Year_Album },
                        new LibraryHierarchyLevel() { Name = "Disk - Track - Title", Sequence = 2, Script = scripts.Disk_Track_Title }
                    }
                });
                transaction.Commit();
            }
        }
    }
}
