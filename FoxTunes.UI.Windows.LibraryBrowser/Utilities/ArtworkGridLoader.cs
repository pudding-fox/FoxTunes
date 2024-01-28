using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ArtworkGridLoader : StandardComponent
    {
        public static readonly object SyncRoot = new object();

        public ArtworkGridLoader()
        {
            this.ForegroundQueue = new List<ArtworkGrid>();
            this.BackgroundQueue = new List<ArtworkGrid>();
        }

        public IList<ArtworkGrid> ForegroundQueue { get; private set; }

        public TaskFactory ForegroundFactory { get; private set; }

        public IList<ArtworkGrid> BackgroundQueue { get; private set; }

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

        public Task Load(ArtworkGrid artworkGrid, ArtworkGridLoaderPriority priority)
        {
            var queue = this.GetQueue(priority);
            var factory = this.GetFactory(priority);
            lock (SyncRoot)
            {
                queue.Add(artworkGrid);
            }
            return factory.StartNew(async () =>
            {
                lock (SyncRoot)
                {
                    if (!queue.Contains(artworkGrid))
                    {
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }
                    queue.Remove(artworkGrid);
                }
                //If we're in the low priority queue then sleep a little so other (more important) threads can work.
                if (priority == ArtworkGridLoaderPriority.Low && this.Interval.Value > 0)
                {
#if NET40
                    await TaskEx.Delay(this.Interval.Value).ConfigureAwait(false);
#else
                    await Task.Delay(this.Interval.Value).ConfigureAwait(false);
#endif
                }
                return artworkGrid.Refresh();
            });
        }

        public void Cancel(ArtworkGrid artworkGrid)
        {
            lock (SyncRoot)
            {
                this.BackgroundQueue.Remove(artworkGrid);
                this.ForegroundQueue.Remove(artworkGrid);
            }
        }

        public void Cancel(ArtworkGrid artworkGrid, ArtworkGridLoaderPriority priority)
        {
            var queue = this.GetQueue(priority);
            lock (SyncRoot)
            {
                queue.Remove(artworkGrid);
            }
        }

        protected virtual IList<ArtworkGrid> GetQueue(ArtworkGridLoaderPriority priority)
        {
            switch (priority)
            {
                case ArtworkGridLoaderPriority.Low:
                    return this.BackgroundQueue;
                default:
                case ArtworkGridLoaderPriority.High:
                    return this.ForegroundQueue;
            }
        }

        protected virtual TaskFactory GetFactory(ArtworkGridLoaderPriority priority)
        {
            switch (priority)
            {
                case ArtworkGridLoaderPriority.Low:
                    return this.BackgroundFactory;
                default:
                case ArtworkGridLoaderPriority.High:
                    return this.ForegroundFactory;
            }
        }
    }

    public enum ArtworkGridLoaderPriority : byte
    {
        None,
        Low,
        High
    }

}