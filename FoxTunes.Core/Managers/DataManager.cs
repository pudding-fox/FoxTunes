using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes.Managers
{
    public class DataManager : StandardManager, IDataManager
    {
        public Task Reload()
        {
            return this.ForegroundTaskRunner.Run(() => this.ReadContext = this.Database.CreateContext());
        }

        public IDatabase Database { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; protected set; }

        private IDatabaseContext _ReadContext { get; set; }

        public IDatabaseContext ReadContext
        {
            get
            {
                return this._ReadContext;
            }
            set
            {
                this._ReadContext = value;
                this.OnReadContextChanged();
            }
        }

        protected virtual void OnReadContextChanged()
        {
            if (this.ReadContextChanged != null)
            {
                this.ReadContextChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ReadContext");
        }

        public event EventHandler ReadContextChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            this.Reload();
            base.InitializeComponent(core);
        }

        public IDatabaseContext CreateWriteContext()
        {
            var context = this.Database.CreateContext();
            context.Disposed += this.OnContextDisposed;
            return context;
        }

        protected virtual void OnContextDisposed(object sender, EventArgs e)
        {
            this.Reload().Wait();
        }
    }
}
