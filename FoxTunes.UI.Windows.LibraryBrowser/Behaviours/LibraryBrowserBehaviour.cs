using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class LibraryBrowserBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return LibraryBrowserBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
