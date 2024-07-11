using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class PlaybackBehaviour : StandardComponent, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PlaybackBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
