using System.Collections.Generic;

namespace FoxTunes
{
    public static class EnqueueNextItemBehaviourConfiguration
    {
        public const string SECTION = PlaybackBehaviourConfiguration.SECTION;

        public static string COUNT = "82DF8CB4-D447-419F-A891-9AC6170B48A1";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            yield return new ConfigurationSection(SECTION)
                .WithElement(
                    new IntegerConfigurationElement(COUNT, "Queue Size").WithValue(1).WithValidationRule(new IntegerValidationRule(1, 16))
            );
        }
    }
}
