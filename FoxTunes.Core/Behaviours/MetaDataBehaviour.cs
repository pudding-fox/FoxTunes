using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class MetaDataBehaviour : StandardComponent, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return MetaDataBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
