using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IConfigurableComponent
    {
        IEnumerable<ConfigurationSection> GetConfigurationSections();
    }
}
