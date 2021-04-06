using System.Collections.Generic;

namespace FoxTunes
{
    public static class UIComponentLayoutProviderConfiguration
    {
        public const string SECTION = WindowsUserInterfaceConfiguration.SECTION;

        public const string LAYOUT = WindowsUserInterfaceConfiguration.LAYOUT_ELEMENT;

        public const string MAIN = "AAAAE4B-657B-45C2-B365-8FD69498D7C2";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new TextConfigurationElement(MAIN, "Main Layout", path: "Advanced\\Layouts")
                    .WithValue(Resources.Main)
                    .DependsOn(SECTION, LAYOUT, UIComponentLayoutProvider.ID)
            );
        }
    }
}
