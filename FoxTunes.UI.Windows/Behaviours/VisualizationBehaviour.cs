using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    //PRIORITY_HIGH: Other components inspect our configuration on startup so make sure it's available.
    [ComponentPriority(ComponentPriorityAttribute.HIGH)]
    [WindowsUserInterfaceDependency]
    public class VisualizationBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return VisualizationBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
