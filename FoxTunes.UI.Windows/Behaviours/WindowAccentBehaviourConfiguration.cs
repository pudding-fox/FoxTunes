using System.Collections.Generic;

namespace FoxTunes
{
    public static class WindowAccentBehaviourConfiguration
    {
        public const string SECTION = WindowsUserInterfaceConfiguration.SECTION;

        public const string ACCENT_COLOR = "OOOO5DBB-8ACE-4FFC-B975-9131D4D82947";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(
                    new TextConfigurationElement(ACCENT_COLOR, Strings.WindowsUserInterfaceConfiguration_AccentColor).DependsOn(SECTION, WindowsUserInterfaceConfiguration.TRANSPARENCY)
            );
        }
    }
}
