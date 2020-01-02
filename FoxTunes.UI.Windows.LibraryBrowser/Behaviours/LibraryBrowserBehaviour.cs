using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LibraryBrowserBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return LibraryBrowserBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
