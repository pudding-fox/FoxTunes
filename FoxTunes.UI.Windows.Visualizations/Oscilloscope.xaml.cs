using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Oscilloscope.xaml
    /// </summary>
    [UIComponent("D3FBE95D-3B9E-4DAB-B3AD-B66A53AF5F85", role: UIComponentRole.Visualization)]
    public partial class Oscilloscope : ConfigurableUIComponentBase
    {
        public const string CATEGORY = "15AC0D00-4359-4DC9-941D-42803AB999DE";

        public Oscilloscope()
        {
            this.InitializeComponent();
        }

        public SelectionConfigurationElement Mode { get; private set; }

        public IntegerConfigurationElement Duration { get; private set; }

        public BooleanConfigurationElement DropShadow { get; private set; }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                    OscilloscopeConfiguration.SECTION,
                    OscilloscopeConfiguration.MODE_ELEMENT
                );
                this.Duration = this.Configuration.GetElement<IntegerConfigurationElement>(
                    OscilloscopeConfiguration.SECTION,
                    OscilloscopeConfiguration.DURATION_ELEMENT
                );
                this.DropShadow = this.Configuration.GetElement<BooleanConfigurationElement>(
                    OscilloscopeConfiguration.SECTION,
                    OscilloscopeConfiguration.DROP_SHADOW_ELEMENT
                );
            }
            base.OnConfigurationChanged();
        }

        public override IEnumerable<string> InvocationCategories
        {
            get
            {
                yield return CATEGORY;
            }
        }

        public override IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                foreach (var option in this.Mode.Options)
                {
                    yield return new InvocationComponent(
                        CATEGORY,
                        option.Id,
                        option.Name,
                        path: this.Mode.Name,
                        attributes: this.Mode.Value == option ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
                for (var value = OscilloscopeConfiguration.DURATION_MIN; value <= OscilloscopeConfiguration.DURATION_MAX; value += 100)
                {
                    yield return new InvocationComponent(
                        CATEGORY,
                        this.Duration.Id,
                        string.Format("{0}ms", value),
                        path: this.Duration.Name,
                        attributes: this.Duration.Value == value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
                yield return new InvocationComponent(
                    CATEGORY,
                    this.DropShadow.Id,
                    this.DropShadow.Name,
                    attributes: this.DropShadow.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
                foreach (var invocationComponent in base.Invocations)
                {
                    yield return invocationComponent;
                }
            }
        }

        public override Task InvokeAsync(IInvocationComponent component)
        {
            if (string.Equals(this.Mode.Name, component.Path))
            {
                this.Mode.Value = this.Mode.Options.FirstOrDefault(option => string.Equals(option.Id, component.Id));
            }
            else if (string.Equals(this.Duration.Name, component.Path))
            {
                var value = default(int);
                if (!string.IsNullOrEmpty(component.Name) && component.Name.Length > 2 && int.TryParse(component.Name.Substring(0, component.Name.Length - 2), out value))
                {
                    this.Duration.Value = value;
                }
                else
                {
                    this.Duration.Value = OscilloscopeConfiguration.DURATION_DEFAULT;
                }
            }
            else if (string.Equals(this.DropShadow.Name, component.Name))
            {
                this.DropShadow.Toggle();
            }
            else if (string.Equals(component.Id, SETTINGS, StringComparison.OrdinalIgnoreCase))
            {
                return this.ShowSettings();
            }
            this.SaveSettings();
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected override Task ShowSettings()
        {
            return this.UserInterface.ShowSettings(
                Strings.OscilloscopeConfiguration_Path,
                this.GetConfiguration(),
                new[]
                {
                    OscilloscopeConfiguration.SECTION
                }
            );
        }

        public override IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return OscilloscopeConfiguration.GetConfigurationSections();
        }
    }
}