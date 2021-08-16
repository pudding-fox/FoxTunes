using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class VisualizationBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return VisualizationBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
