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

        public IStandardFactories Factories
        {
            get
            {
                return StandardFactories.Instance;
            }
        }

        public void Load()
        {
            this.LoadComponents();
            this.LoadManagers();
            this.LoadFactories();
            this.LoadBehaviours();
            this.InitializeComponents();
        }

        protected virtual void LoadComponents()
        {
            ComponentRegistry.Instance.AddComponents(ComponentLoader.Instance.Load());
        }

        protected virtual void LoadManagers()
        {
            ComponentRegistry.Instance.AddComponents(ManagerLoader.Instance.Load());
        }

        protected virtual void LoadFactories()
        {
            ComponentRegistry.Instance.AddComponents(FactoryLoader.Instance.Load());
        }

        protected virtual void LoadBehaviours()
        {
            ComponentRegistry.Instance.AddComponents(BehaviourLoader.Instance.Load());
        }

        protected virtual void InitializeComponents()
        {
            ComponentRegistry.Instance.ForEach(component => component.InitializeComponent(this));
        }
    }
}
