using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class SpectrumBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string BASIC_CATEGORY = "3DF40656-FDD5-4B98-A25C-66DDFFD66CA0";

        public const string ENHANCED_CATEGORY = "13FD94CF-9FBF-460F-9501-571BA7144DCF";

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Bars { get; private set; }

        public SelectionConfigurationElement Bands { get; private set; }

        public BooleanConfigurationElement Peaks { get; private set; }

        public BooleanConfigurationElement Rms { get; private set; }

        public BooleanConfigurationElement Crest { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Bars = this.Configuration.GetElement<SelectionConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.BARS_ELEMENT
            );
            this.Bands = this.Configuration.GetElement<SelectionConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.BANDS_ELEMENT
            );
            this.Peaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.PEAKS_ELEMENT
            );
            this.Rms = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.RMS_ELEMENT
            );
            this.Crest = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.CREST_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                foreach (var option in this.Bars.Options)
                {
                    yield return new InvocationComponent(
                        BASIC_CATEGORY,
                        option.Id,
                        string.Format("{0} {1}", option.Name, Strings.General_Bars),
                        attributes: this.Bars.Value == option ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
                foreach (var option in this.Bands.Options)
                {
                    yield return new InvocationComponent(
                        ENHANCED_CATEGORY,
                        option.Id,
                        string.Format("{0} {1}", option.Name, Strings.General_Bands),
                        attributes: this.Bands.Value == option ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
                yield return new InvocationComponent(
                    BASIC_CATEGORY,
                    this.Peaks.Id,
                    this.Peaks.Name,
                    attributes: (byte)((this.Peaks.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE) | InvocationComponent.ATTRIBUTE_SEPARATOR)
                );
                yield return new InvocationComponent(
                    ENHANCED_CATEGORY,
                    this.Peaks.Id,
                    this.Peaks.Name,
                    attributes: (byte)((this.Peaks.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE) | InvocationComponent.ATTRIBUTE_SEPARATOR)
                );
                yield return new InvocationComponent(
                    ENHANCED_CATEGORY,
                    this.Rms.Id,
                    this.Rms.Name,
                    attributes: this.Rms.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
                yield return new InvocationComponent(
                    ENHANCED_CATEGORY,
                    this.Crest.Id,
                    this.Crest.Name,
                    attributes: this.Crest.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
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
            else if (string.Equals(component.Id, this.Crest.Id, StringComparison.OrdinalIgnoreCase))
            {
                this.Crest.Toggle();
            }
            else
            {
                var bars = this.Bars.Options.FirstOrDefault(option => string.Equals(option.Id, component.Id, StringComparison.OrdinalIgnoreCase));
                if (bars != null)
                {
                    this.Bars.Value = bars;
                }
                var bands = this.Bands.Options.FirstOrDefault(option => string.Equals(option.Id, component.Id, StringComparison.OrdinalIgnoreCase));
                if (bands != null)
                {
                    this.Bands.Value = bands;
                }
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
            return SpectrumBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
