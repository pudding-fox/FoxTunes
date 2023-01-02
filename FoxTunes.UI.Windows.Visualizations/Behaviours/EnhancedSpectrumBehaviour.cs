using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class EnhancedSpectrumBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string CATEGORY = "13FD94CF-9FBF-460F-9501-571BA7144DCF";

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Bands { get; private set; }

        public BooleanConfigurationElement Peaks { get; private set; }

        public BooleanConfigurationElement Rms { get; private set; }

        public BooleanConfigurationElement Crest { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Bands = this.Configuration.GetElement<SelectionConfigurationElement>(
                EnhancedSpectrumBehaviourConfiguration.SECTION,
                EnhancedSpectrumBehaviourConfiguration.BANDS_ELEMENT
            );
            this.Peaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                EnhancedSpectrumBehaviourConfiguration.SECTION,
                EnhancedSpectrumBehaviourConfiguration.PEAKS_ELEMENT
            );
            this.Rms = this.Configuration.GetElement<BooleanConfigurationElement>(
                EnhancedSpectrumBehaviourConfiguration.SECTION,
                EnhancedSpectrumBehaviourConfiguration.RMS_ELEMENT
            );
            this.Crest = this.Configuration.GetElement<BooleanConfigurationElement>(
                EnhancedSpectrumBehaviourConfiguration.SECTION,
                EnhancedSpectrumBehaviourConfiguration.CREST_ELEMENT
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
                foreach (var option in this.Bands.Options)
                {
                    yield return new InvocationComponent(
                        CATEGORY,
                        option.Id,
                        string.Format("{0} {1}", option.Name, this.Bands.Name),
                        path: this.Bands.Name,
                        attributes: this.Bands.Value == option ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
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
                if (this.Rms.Value)
                {
                    yield return new InvocationComponent(
                        CATEGORY,
                        this.Crest.Id,
                        this.Crest.Name,
                        attributes: this.Crest.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
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
            return EnhancedSpectrumBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
