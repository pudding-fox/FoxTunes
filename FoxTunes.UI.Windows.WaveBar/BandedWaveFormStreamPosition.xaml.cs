using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for BandedWaveFormStreamPosition.xaml
    /// </summary>
    [UIComponent("6EDA5DCD-7A00-4933-A2EF-C70C99F7B36A", role: UIComponentRole.Playback)]
    public partial class BandedWaveFormStreamPosition : ConfigurableUIComponentBase
    {
        public const string CATEGORY = "BEE11F64-A91C-461C-9199-98854BF68708";

        public BandedWaveFormStreamPosition()
        {
            this.InitializeComponent();
        }

        public ThemeLoader ThemeLoader { get; private set; }

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
                this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                    BandedWaveFormStreamPositionConfiguration.SECTION,
                    BandedWaveFormStreamPositionConfiguration.COLOR_PALETTE_ELEMENT
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
                foreach (var component in this.ThemeLoader.SelectColorPalette(CATEGORY, this.ColorPalette, ColorPaletteRole.WaveForm))
                {
                    yield return component;
                }
                foreach (var invocationComponent in base.Invocations)
                {
                    yield return invocationComponent;
                }
            }
        }

        public override Task InvokeAsync(IInvocationComponent component)
        {
            if (this.ThemeLoader.SelectColorPalette(this.ColorPalette, component))
            {
                //Nothing to do.
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
                Strings.WaveFormStreamPositionConfiguration_Section,
                BandedWaveFormStreamPositionConfiguration.SECTION
            );
        }

        public override IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BandedWaveFormStreamPositionConfiguration.GetConfigurationSections();
        }
    }
}
