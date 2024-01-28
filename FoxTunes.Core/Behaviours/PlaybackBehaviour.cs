using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class PlaybackBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PlaybackBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
