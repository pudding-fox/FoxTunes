using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LibraryBrowserTileLoader : StandardComponent
    {
        public static readonly object SyncRoot = new object();

        public LibraryBrowserTileLoader()
        {
            this.ForegroundQueue = new List<LibraryBrowserTile>();
            this.BackgroundQueue = new List<LibraryBrowserTile>();
        }

        public IList<LibraryBrowserTile> ForegroundQueue { get; private set; }

        public TaskFactory ForegroundFactory { get; private set; }

        public IList<LibraryBrowserTile> BackgroundQueue { get; private set; }

        public TaskFactory BackgroundFactory { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IntegerConfigurationElement Threads { get; private set; }

        public IntegerConfigurationElement Interval { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Threads = this.Configuration.GetElement<IntegerConfigurationElement>(
                ImageLoaderConfiguration.SECTION,
                ImageLoaderConfiguration.THREADS
            );
            this.Threads.ConnectValue(value =>
            {
                this.ForegroundFactory = new TaskFactory(new TaskScheduler(new ParallelOptions()
                {
                    MaxDegreeOfParallelism = value
                }));
                this.BackgroundFactory = new TaskFactory(new TaskScheduler(new ParallelOptions()
                {
                    MaxDegreeOfParallelism = 1
                }));
            });
            this.Interval = this.Configuration.GetElement<IntegerConfigurationElement>(
                ImageLoaderConfiguration.SECTION,
                ImageLoaderConfiguration.INTERVAL
            );
            base.InitializeComponent(core);
        }

        public Task Load(LibraryBrowserTile libraryBrowserTile, LibraryBrowserTileLoaderPriority priority)
        {
            var queue = this.GetQueue(priority);
            var factory = this.GetFactory(priority);
            lock (SyncRoot)
            {
                queue.Add(libraryBrowserTile);
            }
            return factory.StartNew(async () =>
            {
                lock (SyncRoot)
                {
                    if (!queue.Contains(libraryBrowserTile))
                    {
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }
                    queue.Remove(libraryBrowserTile);
                }
                //If we're in the low priority queue then sleep a little so other (more important) threads can work.
                if (priority == LibraryBrowserTileLoaderPriority.Low && this.Interval.Value > 0)
                {
#if NET40
                    await TaskEx.Delay(this.Interval.Value).ConfigureAwait(false);
#else
                    await Task.Delay(this.Interval.Value).ConfigureAwait(false);
#endif
                }
                return libraryBrowserTile.Refresh();
            });
        }

        public void Cancel(LibraryBrowserTile libraryBrowserTile)
        {
            lock (SyncRoot)
            {
                this.BackgroundQueue.Remove(libraryBrowserTile);
                this.ForegroundQueue.Remove(libraryBrowserTile);
            }
        }

        public void Cancel(LibraryBrowserTile libraryBrowserTile, LibraryBrowserTileLoaderPriority priority)
        {
            var queue = this.GetQueue(priority);
            lock (SyncRoot)
            {
                queue.Remove(libraryBrowserTile);
            }
        }

        protected virtual IList<LibraryBrowserTile> GetQueue(LibraryBrowserTileLoaderPriority priority)
        {
            switch (priority)
            {
                case LibraryBrowserTileLoaderPriority.Low:
                    return this.BackgroundQueue;
                default:
                case LibraryBrowserTileLoaderPriority.High:
                    return this.ForegroundQueue;
            }
        }

        protected virtual TaskFactory GetFactory(LibraryBrowserTileLoaderPriority priority)
        {
            switch (priority)
            {
                case LibraryBrowserTileLoaderPriority.Low:
                    return this.BackgroundFactory;
                default:
                case LibraryBrowserTileLoaderPriority.High:
                    return this.ForegroundFactory;
            }
        }
    }

    public enum LibraryBrowserTileLoaderPriority : byte
    {
        None,
        Low,
        High
    }

}