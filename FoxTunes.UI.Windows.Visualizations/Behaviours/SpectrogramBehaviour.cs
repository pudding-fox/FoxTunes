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

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                return Enumerable.Empty<IInvocationComponent>();
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
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
