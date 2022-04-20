using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Category)
            {
                case BASIC_CATEGORY:
                    this.Bars.Value = this.Bars.Options.FirstOrDefault(option => string.Equals(option.Id, component.Id));
                    break;
                case ENHANCED_CATEGORY:
                    this.Bands.Value = this.Bands.Options.FirstOrDefault(option => string.Equals(option.Id, component.Id));
                    break;
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
