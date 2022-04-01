using System.Collections.Generic;

namespace FoxTunes
{
    public static class MinidiscBehaviourConfiguration
    {
        public const string SECTION = "4F81FE99-1897-4C0D-A978-C2A4790D123D";

        public const string ENABLED = "AAAACAEF-31D9-49B6-A0A9-0B3E55050D0C";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.MinidiscBehaviourConfiguration_Section)
                .WithElement(new BooleanConfigurationElement(ENABLED, Strings.MinidiscBehaviourConfiguration_Enabled).WithValue(false)
            );
        }
    }
}
