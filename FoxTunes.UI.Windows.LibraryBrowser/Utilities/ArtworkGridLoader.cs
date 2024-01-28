using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class ArtworkGridLoader : StandardComponent
    {
        public static readonly object SyncRoot = new object();

        public static TaskScheduler Scheduler = new TaskScheduler(new ParallelOptions()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        });

        public static TaskFactory Factory = new TaskFactory(Scheduler);

        public ArtworkGridLoader()
        {
            this.Queue = new List<ArtworkGrid>();
        }

        public IList<ArtworkGrid> Queue { get; private set; }

        public Task Load(ArtworkGrid artworkGrid)
        {
            lock (SyncRoot)
            {
                this.Queue.Add(artworkGrid);
            }
            return Factory.StartNew(() =>
            {
                lock (SyncRoot)
                {
                    if (!this.Queue.Contains(artworkGrid))
                    {
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }
                }
                return artworkGrid.Refresh();
            });
        }

        public void Cancel(ArtworkGrid artworkGrid)
        {
            lock (SyncRoot)
            {
                this.Queue.Remove(artworkGrid);
            }
        }
    }
}
