using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Spectrogram.xaml
    /// </summary>
    [UIComponent("9AB8D410-B94D-492E-BF00-022A3E77762D", role: UIComponentRole.Visualization)]
    public partial class Spectrogram : ConfigurableUIComponentBase
    {
        public const string CATEGORY = "79A019D3-4DA7-47E1-BED7-318B40B2493E";

        public Spectrogram()
        {
            this.InitializeComponent();
        }

        public SelectionConfigurationElement Mode { get; private set; }

        public SelectionConfigurationElement Scale { get; private set; }

        public IntegerConfigurationElement Smoothing { get; private set; }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                    SpectrogramConfiguration.SECTION,
                    SpectrogramConfiguration.MODE_ELEMENT
                );
                this.Scale = this.Configuration.GetElement<SelectionConfigurationElement>(
                    SpectrogramConfiguration.SECTION,
                    SpectrogramConfiguration.SCALE_ELEMENT
                );
                this.Smoothing = this.Configuration.GetElement<IntegerConfigurationElement>(
                    SpectrogramConfiguration.SECTION,
                    SpectrogramConfiguration.SMOOTHING_ELEMENT
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
                foreach (var option in this.Scale.Options)
                {
                    yield return new InvocationComponent(
                        CATEGORY,
                        option.Id,
                        option.Name,
                        path: this.Scale.Name,
                        attributes: this.Scale.Value == option ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
                for (var value = SpectrogramConfiguration.SMOOTHING_MIN; value <= SpectrogramConfiguration.SMOOTHING_MAX; value++)
                {
                    yield return new InvocationComponent(
                        CATEGORY,
                        this.Smoothing.Id,
                        value == 0 ? Strings.SpectrogramConfiguration_Smoothing_Off : value.ToString(),
                        path: this.Smoothing.Name,
                        attributes: this.Smoothing.Value == value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
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
            else if (string.Equals(this.Scale.Name, component.Path))
            {
                this.Scale.Value = this.Scale.Options.FirstOrDefault(option => string.Equals(option.Id, component.Id));
            }
            else if (string.Equals(this.Smoothing.Name, component.Path))
            {
                var value = default(int);
                if (int.TryParse(component.Name, out value))
                {
                    this.Smoothing.Value = value;
                }
                else
                {
                    if (string.Equals(component.Name, Strings.SpectrogramConfiguration_Smoothing_Off, StringComparison.OrdinalIgnoreCase))
                    {
                        this.Smoothing.Value = SpectrogramConfiguration.SMOOTHING_MIN;
                    }
                    else
                    {
                        this.Smoothing.Value = SpectrogramConfiguration.SMOOTHING_DEFAULT;
                    }
                }
            }
            else if (string.Equals(component.Id, SETTINGS, StringComparison.OrdinalIgnoreCase))
            {
                return this.ShowSettings();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected override Task<bool> ShowSettings()
        {
            return this.ShowSettings(
                Strings.SpectrogramConfiguration_Path,
                SpectrogramConfiguration.SECTION
            );
        }

        public override IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return SpectrogramConfiguration.GetConfigurationSections();
        }
    }
}