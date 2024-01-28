using System.Collections.Generic;

namespace FoxTunes
{
    public static class WindowCoverArtAccentBehaviourConfiguration
    {
        public const string SECTION = WindowsUserInterfaceConfiguration.SECTION;

        public const string ARTWORK_ACCENT = "NNOO79AE-C769-47E6-8F48-52B0784FD5EF";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(
                    new BooleanConfigurationElement(ARTWORK_ACCENT, Strings.WindowsUserInterfaceConfiguration_ArtworkAccent).DependsOn(SECTION, WindowsUserInterfaceConfiguration.TRANSPARENCY)
            );
        }
    }
}
