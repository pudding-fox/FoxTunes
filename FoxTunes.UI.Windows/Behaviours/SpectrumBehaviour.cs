using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class SpectrumBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string CATEGORY = "3DF40656-FDD5-4B98-A25C-66DDFFD66CA0";

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public SelectionConfigurationElement Bars { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.ENABLED_ELEMENT
            );
            this.Bars = this.Configuration.GetElement<SelectionConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.BARS_ELEMENT
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
                        CATEGORY,
                        option.Id,
                        string.Format("{0} Bars", option.Name),
                        attributes: this.Bars.Value == option ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE);
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            this.Bars.Value = this.Bars.Options.FirstOrDefault(option => string.Equals(option.Id, component.Id));
#if NET40
            return TaskEx.Run(() => this.Configuration.Save());
#else
            return Task.Run(() => this.Configuration.Save());
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return SpectrumBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
