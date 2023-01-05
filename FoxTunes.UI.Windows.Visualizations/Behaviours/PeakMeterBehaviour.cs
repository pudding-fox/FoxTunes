using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class PeakMeterBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string CATEGORY = "F58AE444-E7F1-4D3D-9CE6-D1612892CF28";

        public ThemeLoader ThemeLoader { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Peaks { get; private set; }

        public BooleanConfigurationElement Rms { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            this.Configuration = core.Components.Configuration;
            this.Peaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                PeakMeterBehaviourConfiguration.SECTION,
                PeakMeterBehaviourConfiguration.PEAKS
            );
            this.Rms = this.Configuration.GetElement<BooleanConfigurationElement>(
                PeakMeterBehaviourConfiguration.SECTION,
                PeakMeterBehaviourConfiguration.RMS
            );
            this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                PeakMeterBehaviourConfiguration.SECTION,
                PeakMeterBehaviourConfiguration.COLOR_PALETTE
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<string> InvocationCategories
        {
            get
            {
                yield return CATEGORY;
            }
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (!this.Rms.Value)
                {
                    foreach (var component in this.ThemeLoader.SelectColorPalette(CATEGORY, this.ColorPalette))
                    {
                        yield return component;
                    }
                }
                yield return new InvocationComponent(
                    CATEGORY,
                    this.Peaks.Id,
                    this.Peaks.Name,
                    attributes: this.Peaks.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
                yield return new InvocationComponent(
                    CATEGORY,
                    this.Rms.Id,
                    this.Rms.Name,
                    attributes: this.Rms.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            if (string.Equals(component.Id, this.Peaks.Id, StringComparison.OrdinalIgnoreCase))
            {
                this.Peaks.Toggle();
            }
            else if (string.Equals(component.Id, this.Rms.Id, StringComparison.OrdinalIgnoreCase))
            {
                this.Rms.Toggle();
            }
            else if (this.ThemeLoader.SelectColorPalette(this.ColorPalette, component))
            {
                //Nothing to do.
            }
            this.Configuration.Save();
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PeakMeterBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
