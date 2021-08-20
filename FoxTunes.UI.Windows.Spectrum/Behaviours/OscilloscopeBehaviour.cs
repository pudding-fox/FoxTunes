using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class OscilloscopeBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string CATEGORY = "15AC0D00-4359-4DC9-941D-42803AB999DE";

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Mode { get; private set; }

        public IntegerConfigurationElement Duration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                OscilloscopeBehaviourConfiguration.SECTION,
                OscilloscopeBehaviourConfiguration.MODE_ELEMENT
            );
            this.Duration = this.Configuration.GetElement<IntegerConfigurationElement>(
                OscilloscopeBehaviourConfiguration.SECTION,
                OscilloscopeBehaviourConfiguration.DURATION_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                foreach (var option in this.Mode.Options)
                {
                    yield return new InvocationComponent(
                        CATEGORY,
                        option.Id,
                        option.Name,
                        attributes: this.Mode.Value == option ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE);
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            this.Mode.Value = this.Mode.Options.FirstOrDefault(option => string.Equals(option.Id, component.Id));
            this.Configuration.Save();
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return OscilloscopeBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
