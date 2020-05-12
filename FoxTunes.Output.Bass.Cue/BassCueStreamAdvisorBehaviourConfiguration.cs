using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassCueStreamAdvisorBehaviourConfiguration
    {
        public const string SECTION = "220C5CC78-A495-4498-815B-0B415BD685A2";

        public const string ENABLED_ELEMENT = "2E0ADD01-D1E8-4868-BD71-AFACB62FF894";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "CUE")
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled").WithValue(false));
        }
    }
}
