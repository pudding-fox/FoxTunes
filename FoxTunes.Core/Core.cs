using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class Core : BaseComponent, ICore
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
            try
            {
                this.LoadComponents();
                this.LoadManagers();
                this.LoadFactories();
                this.LoadBehaviours();
                this.LoadConfiguration();
                this.InitializeComponents();
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Debug, "Failed to initialize the core, we will crash soon: {0}", e.Message);
                throw;
            }
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

        protected virtual void LoadConfiguration()
        {
            ComponentRegistry.Instance.ForEach<IConfigurableComponent>(component =>
            {
                try
                {
                    var sections = (component as IConfigurableComponent).GetConfigurationSections();
                    foreach (var section in sections)
                    {
                        this.Components.Configuration.RegisterSection(section);
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to register configuration for component {0}: {1}", component.GetType().Name, e.Message);
                }
            });
        }

        protected virtual void InitializeComponents()
        {
            ComponentRegistry.Instance.ForEach(component =>
            {
                try
                {
                    component.InitializeComponent(this);
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to initialize component {0}: {1}", component.GetType().Name, e.Message);
                }
            });
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            ComponentRegistry.Instance.ForEach<IDisposable>(component => component.Dispose());
            ComponentRegistry.Instance.Clear();
        }

        ~Core()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
