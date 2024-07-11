using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class PlaylistGroupingBehaviour : StandardComponent, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PlaylistGroupingBehaviourConfiguration.GetConfigurationSections();
        }
    }
}