using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for WaveFormStreamPosition.xaml
    /// </summary>
    [UIComponent("1CABFBE6-C5BD-4818-A092-2D79509D3A52", role: UIComponentRole.Playback)]
    public partial class WaveFormStreamPosition : ConfigurableUIComponentBase
    {
        public const string CATEGORY = "0E698392-FF2C-415A-BB6E-754604DFAB57";

        public WaveFormStreamPosition()
        {
            this.InitializeComponent();
        }

        public ThemeLoader ThemeLoader { get; private set; }

        public SelectionConfigurationElement Mode { get; private set; }

        public BooleanConfigurationElement Rms { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        protected override void InitializeComponent(ICore core)
        {
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            base.InitializeComponent(core);
        }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                    WaveFormStreamPositionConfiguration.SECTION,
                    WaveFormStreamPositionConfiguration.MODE_ELEMENT
                );
                this.Rms = this.Configuration.GetElement<BooleanConfigurationElement>(
                    WaveFormStreamPositionConfiguration.SECTION,
                    WaveFormStreamPositionConfiguration.RMS_ELEMENT
                );
                this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                    WaveFormStreamPositionConfiguration.SECTION,
                    WaveFormStreamPositionConfiguration.COLOR_PALETTE_ELEMENT
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
                if (this.ColorPalette.IsVisible)
                {
                    foreach (var component in this.ThemeLoader.SelectColorPalette(CATEGORY, this.ColorPalette))
                    {
                        yield return component;
                    }
                }
                yield return new InvocationComponent(
                    CATEGORY,
                    this.Rms.Id,
                    this.Rms.Name,
                    attributes: this.Rms.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
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
            else if (string.Equals(this.Rms.Name, component.Name))
            {
                this.Rms.Toggle();
            }
            else if (this.ThemeLoader.SelectColorPalette(this.ColorPalette, component))
            {
                //Nothing to do.
            }
            else if (string.Equals(component.Id, SETTINGS, StringComparison.OrdinalIgnoreCase))
            {
                return this.ShowSettings();
            }
            this.Configuration.Save();
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected override Task ShowSettings()
        {
            return this.UserInterface.ShowSettings(
                Strings.WaveFormStreamPositionConfiguration_Section,
                this.GetConfiguration(),
                new[]
                {
                    WaveFormStreamPositionConfiguration.SECTION
                }
            );
        }

        public override IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WaveFormStreamPositionConfiguration.GetConfigurationSections();
        }
    }
}
