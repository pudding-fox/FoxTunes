using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassArchiveStreamProviderBehaviourConfiguration
    {
        public const string SECTION = "B593BE99-CCA4-42D2-A129-7F65A96FD302";

        public const string ENABLED_ELEMENT = "AAAA73E0-DAAD-4B73-A375-D6D820AFAE93";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Archive")
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled").WithValue(false));
        }
    }
}
