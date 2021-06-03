using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassArchiveStreamProviderBehaviourConfiguration
    {
        public const string SECTION = "B593BE99-CCA4-42D2-A129-7F65A96FD302";

        public const string ENABLED_ELEMENT = "AAAA73E0-DAAD-4B73-A375-D6D820AFAE93";

        public const string METADATA_ELEMENT = "BBBB68A8-AC18-4FBF-8C87-DE24EA49262C";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.BassArchiveStreamProviderBehaviourConfiguration_Section)
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, Strings.BassArchiveStreamProviderBehaviourConfiguration_Enabled)
                    .WithValue(false))
                .WithElement(new BooleanConfigurationElement(METADATA_ELEMENT, Strings.BassArchiveStreamProviderBehaviourConfiguration_MetaData)
                    .WithValue(true)
                    .DependsOn(SECTION, ENABLED_ELEMENT)
                    .DependsOn(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT));
        }
    }
}
