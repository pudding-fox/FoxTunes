using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LayoutManager : StandardComponent, IDisposable
    {
        public static readonly Type PLACEHOLDER = typeof(object);

        private static readonly Lazy<IEnumerable<UIComponent>> _Components = new Lazy<IEnumerable<UIComponent>>(() =>
        {
            var components = new List<UIComponent>();
            foreach (var type in ComponentScanner.Instance.GetComponents(typeof(IUIComponent)))
            {
                var attribute = default(UIComponentAttribute);
                if (!type.HasCustomAttribute<UIComponentAttribute>(false, out attribute))
                {
                    //We don't really want to expose components without annotations.
                    //attribute = new UIComponentAttribute(type.AssemblyQualifiedName, UIComponentSlots.NONE, type.Name);
                    continue;
                }
                components.Add(new UIComponent(attribute, type));
            }
            return components.OrderBy(
                component => component.Name
            ).ToArray();
        });

        public LayoutManager()
        {
            this.Providers = new HashSet<IUILayoutProvider>();
            Instance = this;
        }

        public HashSet<IUILayoutProvider> Providers { get; private set; }

        public IUILayoutProvider Provider
        {
            get
            {
                return this.Providers.FirstOrDefault(
                    provider => string.Equals(provider.Id, this.Layout, StringComparison.OrdinalIgnoreCase)
                ) ?? this.Providers.FirstOrDefault();
            }
        }

        public IEnumerable<UIComponent> Components
        {
            get
            {
                return _Components.Value;
            }
        }

        public bool IsComponentActive(string id)
        {
            var provider = this.Provider;
            if (provider == null)
            {
                return false;
            }
            return provider.IsComponentActive(id);
        }

        public IConfiguration Configuration { get; private set; }

        private string _Layout { get; set; }

        public string Layout
        {
            get
            {
                return this._Layout;
            }
            set
            {
                this._Layout = value;
                this.OnLayoutChanged();
            }
        }

        protected virtual void OnLayoutChanged()
        {
            if (this.LayoutChanged != null)
            {
                this.LayoutChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Layout");
        }

        public event EventHandler LayoutChanged;

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT
            ).ConnectValue(value => this.Layout = value.Id);
            base.InitializeComponent(core);
        }

        public UIComponent GetComponent(string id)
        {
            return this.Components.FirstOrDefault(component => string.Equals(component.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public void Register(IUILayoutProvider layoutProvider)
        {
            this.Providers.Add(layoutProvider);
        }

        public UIComponentBase Load(UILayoutTemplate template)
        {
            var provider = this.Provider;
            if (provider == null)
            {
                return null;
            }
            return provider.Load(template);
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

        }

        ~LayoutManager()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        public static LayoutManager Instance { get; private set; }
    }
}
