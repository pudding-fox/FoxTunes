using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class SpectrogramBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string CATEGORY = "79A019D3-4DA7-47E1-BED7-318B40B2493E";

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Mode { get; private set; }

        public SelectionConfigurationElement Scale { get; private set; }

        public IntegerConfigurationElement Smoothing { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                SpectrogramBehaviourConfiguration.SECTION,
                SpectrogramBehaviourConfiguration.MODE_ELEMENT
            );
            this.Scale = this.Configuration.GetElement<SelectionConfigurationElement>(
                SpectrogramBehaviourConfiguration.SECTION,
                SpectrogramBehaviourConfiguration.SCALE_ELEMENT
            );
            this.Smoothing = this.Configuration.GetElement<IntegerConfigurationElement>(
                SpectrogramBehaviourConfiguration.SECTION,
                SpectrogramBehaviourConfiguration.SMOOTHING_ELEMENT
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
                foreach (var option in this.Mode.Options)
                {
                    yield return new InvocationComponent(
                        CATEGORY,
                        option.Id,
                        option.Name,
                        path: this.Mode.Name,
                        attributes: this.Mode.Value == option ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE);
                }
                foreach (var option in this.Scale.Options)
                {
                    yield return new InvocationComponent(
                        CATEGORY,
                        option.Id,
                        option.Name,
                        path: this.Scale.Name,
                        attributes: this.Scale.Value == option ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE);
                }
                for (var value = SpectrogramBehaviourConfiguration.SMOOTHING_MIN; value <= SpectrogramBehaviourConfiguration.SMOOTHING_MAX; value++)
                {
                    yield return new InvocationComponent(
                        CATEGORY,
                        this.Smoothing.Id,
                        value == 0 ? Strings.SpectrogramBehaviourConfiguration_Smoothing_Off : value.ToString(),
                        path: this.Smoothing.Name,
                        attributes: this.Smoothing.Value == value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE);
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
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
                    this.Smoothing.Value = SpectrogramBehaviourConfiguration.SMOOTHING_DEFAULT;
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
            return SpectrogramBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
