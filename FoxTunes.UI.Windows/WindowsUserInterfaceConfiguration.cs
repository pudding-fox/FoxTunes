using FoxTunes.Theme;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static class WindowsUserInterfaceConfiguration
    {
        public const string APPEARANCE_SECTION = "0047011D-7C95-4EDE-A4DE-B839CF05E9AB";

        public const string THEME_ELEMENT = "06189DEE-1168-4D96-9355-31ECC0666820";

        public const string SYSTEM_SECTION = "FE07ADAE-393F-48E8-90E3-260F28B1189E";

        public const string LOGGING_ELEMENT = "6FFC8459-F9D9-44CF-A3AB-42832DB04C17";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var themeOptions = GetThemeOptions().ToArray();
            yield return new ConfigurationSection(APPEARANCE_SECTION, "Appearance")
                .WithElement(
                    new SelectionConfigurationElement(THEME_ELEMENT, "Theme")
                    {
                        SelectedOption = themeOptions.FirstOrDefault()
                    }.WithOptions(() => themeOptions)
            );
            yield return new ConfigurationSection(SYSTEM_SECTION, "System")
                .WithElement(
                    new BooleanConfigurationElement(LOGGING_ELEMENT, "Logging")
                        .WithValue(false)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetThemeOptions()
        {
            var themes = ComponentRegistry.Instance.GetComponents<ITheme>();
            foreach (var theme in themes)
            {
                yield return new SelectionConfigurationOption(theme.Id, theme.Name, theme.Description);
            }
        }
    }
}
