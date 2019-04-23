using System.Collections.Generic;

namespace FoxTunes
{
    public static class SystemMediaTransportControlsBehaviourConfiguration
    {
        public const string SECTION = "B545D1D1-E8A8-4DED-B359-3BDA3DC9CBFF";

        public const string ENABLED_ELEMENT = "AAAA69E1-80B1-46BD-BE24-BC56C5A04141";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "System Media Controls")
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled").WithValue(false)
            );
        }
    }
}
