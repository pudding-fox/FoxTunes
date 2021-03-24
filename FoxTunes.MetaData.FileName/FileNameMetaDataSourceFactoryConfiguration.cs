using System.Collections.Generic;

namespace FoxTunes
{
    public static class FileNameMetaDataSourceFactoryConfiguration
    {
        public const string PATTERNS_ELEMENT = "AAAA5E9E-DEEB-4872-B9B9-A20A65D16259";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(MetaDataBehaviourConfiguration.SECTION)
                .WithElement(new TextConfigurationElement(PATTERNS_ELEMENT, Strings.FileNameMetaDataSourceFactoryConfiguration_Patterns, path: Strings.FileNameMetaDataSourceFactoryConfiguration_Path).WithValue(Resources.Patterns).WithFlags(ConfigurationElementFlags.MultiLine)
            );
        }
    }
}
