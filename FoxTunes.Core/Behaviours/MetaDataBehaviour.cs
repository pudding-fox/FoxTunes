using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class MetaDataBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return MetaDataBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
