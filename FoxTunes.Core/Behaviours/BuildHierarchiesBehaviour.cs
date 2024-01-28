using FoxTunes.Interfaces;

namespace FoxTunes.Behaviours
{
    public class BuildHierarchiesBehaviour : StandardBehaviour
    {
        public ICore Core { get; private set; }

        public IHierarchyManager HierarchyManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.HierarchyManager = core.Managers.Hierarchy;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.InitializeComponent(core);
        }

        private void OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.LibraryUpdated:
                    this.HierarchyManager.BuildHierarchies();
                    break;
            }
        }
    }
}
