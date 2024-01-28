using System.Collections.Generic;

namespace FoxTunes
{
    public static class FileNameMetaDataSourceFactoryConfiguration
    {
        public const string SECTION = "04DAB74A-BB40-4246-A119-DD147645EB34";

        public const string PATTERNS_ELEMENT = "ECDA5E9E-DEEB-4872-B9B9-A20A65D16259";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "File Name Meta Data")
                .WithElement(new TextConfigurationElement(PATTERNS_ELEMENT, "Pattern").WithValue(Resources.Patterns)
            );
        }
    }
}
