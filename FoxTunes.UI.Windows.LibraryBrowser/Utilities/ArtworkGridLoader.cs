using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class ArtworkGridLoader : StandardComponent
    {
        public static readonly object SyncRoot = new object();

        public ArtworkGridLoader()
        {
            this.Queue = new List<ArtworkGrid>();
        }

        public TaskFactory Factory { get; private set; }

        public IList<ArtworkGrid> Queue { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<IntegerConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.THREADS_ELEMENT
            ).ConnectValue(value =>
            {
                this.Factory = new TaskFactory(new TaskScheduler(new ParallelOptions()
                {
                    MaxDegreeOfParallelism = value
                }));
            });
            base.InitializeComponent(core);
        }



        public Task Load(ArtworkGrid artworkGrid)
        {
            lock (SyncRoot)
            {
                this.Queue.Add(artworkGrid);
            }
            return this.Factory.StartNew(() =>
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
