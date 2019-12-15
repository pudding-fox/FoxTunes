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

        public LayoutManager()
        {
            this._Components = new Lazy<IEnumerable<UIComponent>>(this.GetComponents);
            Instance = this;
        }

        public IConfiguration Configuration { get; private set; }

        private SelectionConfigurationElement _TopLeft { get; set; }

        public SelectionConfigurationElement TopLeft
        {
            get
            {
                return this._TopLeft;
            }
            set
            {
                this._TopLeft = value;
                this.OnTopLeftChanged();
            }
        }

        protected virtual void OnTopLeftChanged()
        {
            if (this.TopLeftChanged != null)
            {
                this.TopLeftChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("TopLeft");
        }

        public event EventHandler TopLeftChanged;

        private SelectionConfigurationElement _BottomLeft { get; set; }

        public SelectionConfigurationElement BottomLeft
        {
            get
            {
                return this._BottomLeft;
            }
            set
            {
                this._BottomLeft = value;
                this.OnBottomLeftChanged();
            }
        }

        protected virtual void OnBottomLeftChanged()
        {
            if (this.BottomLeftChanged != null)
            {
                this.BottomLeftChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("BottomLeft");
        }

        public event EventHandler BottomLeftChanged;

        private SelectionConfigurationElement _TopCenter { get; set; }

        public SelectionConfigurationElement TopCenter
        {
            get
            {
                return this._TopCenter;
            }
            set
            {
                this._TopCenter = value;
                this.OnTopCenterChanged();
            }
        }

        protected virtual void OnTopCenterChanged()
        {
            if (this.TopCenterChanged != null)
            {
                this.TopCenterChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("TopCenter");
        }

        public event EventHandler TopCenterChanged;

        private SelectionConfigurationElement _BottomCenter { get; set; }

        public SelectionConfigurationElement BottomCenter
        {
            get
            {
                return this._BottomCenter;
            }
            set
            {
                this._BottomCenter = value;
                this.OnBottomCenterChanged();
            }
        }

        protected virtual void OnBottomCenterChanged()
        {
            if (this.BottomCenterChanged != null)
            {
                this.BottomCenterChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("BottomCenter");
        }

        public event EventHandler BottomCenterChanged;

        private SelectionConfigurationElement _TopRight { get; set; }

        public SelectionConfigurationElement TopRight
        {
            get
            {
                return this._TopRight;
            }
            set
            {
                this._TopRight = value;
                this.OnTopRightChanged();
            }
        }

        protected virtual void OnTopRightChanged()
        {
            if (this.TopRightChanged != null)
            {
                this.TopRightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("TopRight");
        }

        public event EventHandler TopRightChanged;

        private SelectionConfigurationElement _BottomRight { get; set; }

        public SelectionConfigurationElement BottomRight
        {
            get
            {
                return this._BottomRight;
            }
            set
            {
                this._BottomRight = value;
                this.OnBottomRightChanged();
            }
        }

        protected virtual void OnBottomRightChanged()
        {
            if (this.BottomRightChanged != null)
            {
                this.BottomRightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("BottomRight");
        }

        public event EventHandler BottomRightChanged;

        public IEnumerable<Type> ActiveComponents
        {
            get
            {
                var elements = new[]
                {
                    this.TopLeft,
                    this.BottomLeft,
                    this.TopCenter,
                    this.BottomCenter,
                    this.TopRight,
                    this.BottomRight
                };
                foreach (var element in elements)
                {
                    var control = WindowsUserInterfaceConfiguration.GetControl(element.Value);
                    if (control != null)
                    {
                        yield return control;
                    }
                }
            }
        }

        protected virtual void OnActiveComponentsChanged()
        {
            if (this.ActiveComponentsChanged != null)
            {
                this.ActiveComponentsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ActiveComponents");
        }

        public event EventHandler ActiveComponentsChanged;

        public bool IsComponentActive(Type type)
        {
            return this.ActiveComponents.Any(component => string.Equals(component.AssemblyQualifiedName, type.AssemblyQualifiedName, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsComponentValid(Type type)
        {
            var attributes = default(IEnumerable<UIComponentDependencyAttribute>);
            if (type.HasCustomAttributes<UIComponentDependencyAttribute>(false, out attributes))
            {
                foreach (var attribute in attributes)
                {
                    var element = this.Configuration.GetElement<BooleanConfigurationElement>(
                        attribute.Section,
                        attribute.Element
                    );
                    if (element == null || !element.Value)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void ConnectComponentValid(UIComponentBase component)
        {
            var type = component.GetType();
            var attributes = default(IEnumerable<UIComponentDependencyAttribute>);
            if (type.HasCustomAttributes<UIComponentDependencyAttribute>(false, out attributes))
            {
                foreach (var attribute in attributes)
                {
                    var element = this.Configuration.GetElement<BooleanConfigurationElement>(
                        attribute.Section,
                        attribute.Element
                    );
                    element.ConnectValue(value => component.IsComponentValid = this.IsComponentValid(type));
                }
            }
        }

        private Lazy<IEnumerable<UIComponent>> _Components { get; set; }

        public IEnumerable<UIComponent> Components
        {
            get
            {
                return this._Components.Value;
            }
        }

        private IEnumerable<UIComponent> GetComponents()
        {
            var components = new List<UIComponent>();
            foreach (var type in ComponentScanner.Instance.GetComponents(typeof(IUIComponent)))
            {
                var attribute = default(UIComponentAttribute);
                if (!type.HasCustomAttribute<UIComponentAttribute>(false, out attribute))
                {
                    attribute = new UIComponentAttribute(type.AssemblyQualifiedName, UIComponentSlots.NONE, type.Name);
                }
                components.Add(new UIComponent(attribute, type));
            }
            return components;
        }

        public UIComponent GetComponent(string id)
        {
            return this.Components.FirstOrDefault(component => string.Equals(component.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.TopLeft = this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.TOP_LEFT_ELEMENT
            );
            this.BottomLeft = this.Configuration.GetElement<SelectionConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.BOTTOM_LEFT_ELEMENT
            );
            this.TopCenter = this.Configuration.GetElement<SelectionConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.TOP_CENTER_ELEMENT
            );
            this.BottomCenter = this.Configuration.GetElement<SelectionConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.BOTTOM_CENTER_ELEMENT
            );
            this.TopRight = this.Configuration.GetElement<SelectionConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.TOP_RIGHT_ELEMENT
            );
            this.BottomRight = this.Configuration.GetElement<SelectionConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.BOTTOM_RIGHT_ELEMENT
            );
            if (this.TopLeft != null)
            {
                this.TopLeft.ValueChanged += this.OnLayoutUpdated;
            }
            if (this.BottomLeft != null)
            {
                this.BottomLeft.ValueChanged += this.OnLayoutUpdated;
            }
            if (this.TopCenter != null)
            {
                this.TopCenter.ValueChanged += this.OnLayoutUpdated;
            }
            if (this.BottomCenter != null)
            {
                this.BottomCenter.ValueChanged += this.OnLayoutUpdated;
            }
            if (this.TopRight != null)
            {
                this.TopRight.ValueChanged += this.OnLayoutUpdated;
            }
            if (this.BottomRight != null)
            {
                this.BottomRight.ValueChanged += this.OnLayoutUpdated;
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnLayoutUpdated(object sender, EventArgs e)
        {
            this.OnActiveComponentsChanged();
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
            if (this.TopLeft != null)
            {
                this.TopLeft.ValueChanged -= this.OnLayoutUpdated;
            }
            if (this.BottomLeft != null)
            {
                this.BottomLeft.ValueChanged -= this.OnLayoutUpdated;
            }
            if (this.TopCenter != null)
            {
                this.TopCenter.ValueChanged -= this.OnLayoutUpdated;
            }
            if (this.BottomCenter != null)
            {
                this.BottomCenter.ValueChanged -= this.OnLayoutUpdated;
            }
            if (this.TopRight != null)
            {
                this.TopRight.ValueChanged -= this.OnLayoutUpdated;
            }
            if (this.BottomRight != null)
            {
                this.BottomRight.ValueChanged -= this.OnLayoutUpdated;
            }
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
