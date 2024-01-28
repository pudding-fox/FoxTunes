using FoxTunes.Interfaces;

namespace FoxTunes.Behaviours
{
    [Component("5E368C0F-E83D-42D7-AB92-8AB68016A5F2", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_LOW)]
    public class BuildHierarchiesBehaviour : StandardBehaviour
    {
        public IHierarchyManager HierarchyManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.HierarchyManager = core.Managers.Hierarchy;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.InitializeComponent(core);
        }

        protected virtual void OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.LibraryUpdated:
                    this.OnRun();
                    break;
            }
        }

        protected virtual void OnRun()
        {
            this.HierarchyManager.BuildHierarchies();
        }
    }
}
