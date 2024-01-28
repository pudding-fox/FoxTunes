using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class LibraryTaskBase : BackgroundTask
    {
        protected LibraryTaskBase(string id, bool visible = true)
            : base(id, visible)
        {
        }

        public IDataManager DataManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.DataManager = core.Managers.Data;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected virtual Task SaveChanges(IDatabaseContext context, bool showProgress)
        {
            if (showProgress)
            {
                this.Name = "Saving changes";
                this.IsIndeterminate = true;
            }
            Logger.Write(this, LogLevel.Debug, "Saving changes to library.");
            return context.SaveChangesAsync();
        }
    }
}
