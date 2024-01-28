using System.Collections.Generic;

namespace FoxTunes
{
    public static class LibraryBrowserBehaviourConfiguration
    {
        public const string LIBRARY_BROWSER_TILE_SIZE = "HHHH0E5E-FB67-43D3-AE30-BF7571A1A8B1";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(WindowsUserInterfaceConfiguration.SECTION, "Appearance")
                .WithElement(
                    new IntegerConfigurationElement(LIBRARY_BROWSER_TILE_SIZE, "Library Tile Size", path: "Advanced").WithValue(160).WithValidationRule(new IntegerValidationRule(60, 300, 4))
            );
        }
    }
}
