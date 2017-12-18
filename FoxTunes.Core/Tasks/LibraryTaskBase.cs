using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class LibraryTaskBase : BackgroundTask
    {
        protected LibraryTaskBase(string id)
            : base(id)
        {
        }

        public IDatabase Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Database = core.Components.Database;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }
    }
}
