using System.Collections.Generic;

namespace FoxTunes
{
    public static class AdamantineThemeConfiguration
    {
        public const string SECTION = WindowsUserInterfaceConfiguration.SECTION;

        public const string THEME = WindowsUserInterfaceConfiguration.THEME_ELEMENT;

        public const string LIST_ROW_SHADING = "TTTT0874-E4D0-445B-9DA0-3C7344FEB412";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new BooleanConfigurationElement(LIST_ROW_SHADING, Strings.AdamantineThemeConfiguration_ListRowShading, path: Strings.General_Advanced)
                    .WithValue(true)
                    .DependsOn(SECTION, THEME, AdamantineTheme.ID)
            );
        }
    }
}
