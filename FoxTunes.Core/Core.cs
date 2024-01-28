using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class Core : ICore
    {
        public IStandardComponents Components
        {
            get
            {
                return StandardComponents.Instance;
            }
        }

        public IStandardManagers Managers
        {
            get
            {
                return StandardManagers.Instance;
            }
        }

        public void LoadComponents()
        {
            ComponentRegistry.Instance.AddComponents(ComponentLoader.Instance.Load());
        }

        public void LoadManagers()
        {
            ComponentRegistry.Instance.AddComponents(ManagerLoader.Instance.Load());
        }

        public void LoadBehaviours()
        {
            ComponentRegistry.Instance.AddComponents(BehaviourLoader.Instance.Load());
        }

        public void InitializeComponents()
        {
            ComponentRegistry.Instance.ForEach(component => component.InitializeComponent(this));
        }
    }
}
