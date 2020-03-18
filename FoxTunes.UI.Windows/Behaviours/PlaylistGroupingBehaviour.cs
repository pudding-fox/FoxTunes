#if NET40

//ListView grouping is too slow under net40 due to lack of virtualization.

#else

using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistGroupingBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PlaylistGroupingBehaviourConfiguration.GetConfigurationSections();
        }
    }
}

#endif