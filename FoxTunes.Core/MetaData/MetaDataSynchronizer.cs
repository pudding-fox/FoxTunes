using FoxTunes.Interfaces;
using System.Threading.Tasks;
using System.Timers;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class MetaDataSynchronizer : StandardComponent, IMetaDataSynchronizer
    {
        const int UPDATE_INTERVAL = 60000;

        public static readonly object SyncRoot = new object();

        public IMetaDataManager MetaDataManager { get; private set; }

        public global::System.Timers.Timer Timer { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.WRITE_ELEMENT
            ).ConnectValue(value =>
            {
                switch (MetaDataBehaviourConfiguration.GetWriteBehaviour(value))
                {
                    case WriteBehaviour.None:
                        this.Disable();
                        break;
                    default:
                        this.Enable();
                        break;
                }
            });
            base.InitializeComponent(core);
        }

        private void Enable()
        {
            lock (SyncRoot)
            {
                this.Timer = new global::System.Timers.Timer();
                this.Timer.Interval = UPDATE_INTERVAL;
                this.Timer.AutoReset = false;
                this.Timer.Elapsed += this.OnElapsed;
                this.Timer.Start();
            }
        }

        private void Disable()
        {
            lock (SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Stop();
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                    this.Timer = null;
                }
            }
        }

        protected virtual async void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await this.MetaDataManager.Synchronize().ConfigureAwait(false);
                lock (SyncRoot)
                {
                    if (this.Timer == null)
                    {
                        return;
                    }
                    this.Timer.Start();
                }
            }
            catch
            {
                //Nothing can be done, never throw on background thread.
            }
        }

        public Task Synchronize(params LibraryItem[] libraryItem)
        {
            return this.MetaDataManager.Synchronize(libraryItem);
        }

        public Task Synchronize(params PlaylistItem[] playlistItem)
        {
            return this.MetaDataManager.Synchronize(playlistItem);
        }
    }
}
