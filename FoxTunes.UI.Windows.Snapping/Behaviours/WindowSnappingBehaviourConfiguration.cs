using System.Collections.Generic;

namespace FoxTunes
{
    public static class WindowSnappingBehaviourConfiguration
    {
        public const string SECTION = "735BD4BE-5E73-4DE1-A5C7-10058059B436";

        public const string ENABLED = "AAAAEC3B-7C68-4CCC-AB7A-11257BC30374";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Window Snapping")
                .WithElement(new BooleanConfigurationElement(ENABLED, "Enabled").WithValue(false));
        }
    }
}
