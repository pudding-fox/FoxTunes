using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class SpectrumBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return SpectrumBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
