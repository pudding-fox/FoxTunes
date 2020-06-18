using ManagedBass.Cd;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassCdStreamProviderBehaviourConfiguration
    {
        public const string SECTION = "220BF762-28B1-436C-951D-5B0359473A40";

        public const string ENABLED_ELEMENT = "AAAA1DA2-B933-42AC-8FE7-64BA7D7EA2B8";

        public const string LOOKUP_ELEMENT = "BBBB29AB-ED22-4AB2-AD79-0EFE4EAB39B7";

        public const string LOOKUP_HOST_ELEMENT = "CCCC87EE-07E2-4B95-8F9D-039738956A30";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "CD")
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled").WithValue(false))
                .WithElement(new BooleanConfigurationElement(LOOKUP_ELEMENT, "Lookup Tags")
                    .WithValue(true)
                    .DependsOn(SECTION, ENABLED_ELEMENT))
                .WithElement(new TextConfigurationElement(LOOKUP_HOST_ELEMENT, "Host")
                    .WithValue(BassCd.CDDBServer)
                    .DependsOn(SECTION, ENABLED_ELEMENT).DependsOn(SECTION, LOOKUP_ELEMENT));
        }
    }
}
