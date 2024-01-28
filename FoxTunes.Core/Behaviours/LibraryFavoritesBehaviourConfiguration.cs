using System.Collections.Generic;

namespace FoxTunes
{
    public static class LibraryFavoritesBehaviourConfiguration
    {
        public const string SECTION = LibraryBehaviourConfiguration.SECTION;

        public const string ENABLED_ELEMENT = "AAAA21E1-9672-4383-9881-1B490BDC9575";

        public const string SHOW_FAVORITES_ELEMENT = "BBBB8A2D-9161-479B-B6DA-29D5B232946D";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Library")
                .WithElement(
                    new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled", path: "Favorites").WithValue(true))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_FAVORITES_ELEMENT, "Show Only Favorites", path: "Favorites").WithValue(false)
            );
            StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(SECTION, ENABLED_ELEMENT).ConnectValue(value => UpdateConfiguration(value));
        }

        private static void UpdateConfiguration(bool enabled)
        {
            if (enabled)
            {
                StandardComponents.Instance.Configuration.GetElement(SECTION, SHOW_FAVORITES_ELEMENT).Show();
            }
            else
            {
                StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(SECTION, SHOW_FAVORITES_ELEMENT).Do(element =>
                {
                    if (element.Value)
                    {
                        element.Toggle();
                    }
                    element.Hide();
                });
            }
        }
    }
}
