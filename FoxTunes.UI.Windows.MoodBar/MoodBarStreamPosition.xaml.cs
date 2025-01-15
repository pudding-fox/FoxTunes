using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for MoodBarStreamPosition.xaml
    /// </summary>
    [UIComponent("B54A1FAA-F507-4A17-9621-FC324AFF4CFE", role: UIComponentRole.Playback)]
    [UIComponentToolbar(1000, UIComponentToolbarAlignment.Stretch, false)]
    public partial class MoodBarStreamPosition : ConfigurableUIComponentBase
    {
        public const string CATEGORY = "BEE11F64-A91C-461C-9199-98854BF68708";

        public MoodBarStreamPosition()
        {
            this.InitializeComponent();
        }

        protected override void InitializeComponent(ICore core)
        {
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            base.InitializeComponent(core);
        }

        public ThemeLoader ThemeLoader { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                    MoodBarStreamPositionConfiguration.SECTION,
                    MoodBarStreamPositionConfiguration.COLOR_PALETTE_ELEMENT
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
                Strings.MoodBarStreamPositionConfiguration_Section,
                MoodBarStreamPositionConfiguration.SECTION
            );
        }

        public override IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return MoodBarStreamPositionConfiguration.GetConfigurationSections();
        }
    }
}
