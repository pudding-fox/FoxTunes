using System.Collections.Generic;

namespace FoxTunes.UI.Windows.Themes
{
    internal class TransparentThemeConfiguration
    {
        public const string SECTION = WindowsUserInterfaceConfiguration.SECTION;

        public const string OPACITY = "AABBBC99-41C7-4563-BB7E-12EDAC59E57F";

        public const int DEFAULT_OPACITY = 30;

        public const int MIN_OPACITY = 1;

        public const int MAX_OPACITY = 100;

        public const string ACCENT_COLOR = WindowsUserInterfaceConfiguration.ACCENT_COLOR;

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new IntegerConfigurationElement(OPACITY, Strings.TransparentThemeConfiguration_Opacity)
                    .WithValue(DEFAULT_OPACITY)
                    .WithValidationRule(new IntegerValidationRule(MIN_OPACITY, MAX_OPACITY))
                    .DependsOn(WindowsUserInterfaceConfiguration.SECTION, WindowsUserInterfaceConfiguration.THEME_ELEMENT, TransparentTheme.ID))
                 .WithElement(new TextConfigurationElement(ACCENT_COLOR, Strings.TransparentThemeConfiguration_AccentColor)
                    .DependsOn(WindowsUserInterfaceConfiguration.SECTION, WindowsUserInterfaceConfiguration.THEME_ELEMENT, TransparentTheme.ID));
        }
    }
}
