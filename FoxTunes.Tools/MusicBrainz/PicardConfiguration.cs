using System.Collections.Generic;
using System.IO;

namespace FoxTunes
{
    public static class PicardConfiguration
    {
        const string PATH = "C:\\Program Files\\MusicBrainz Picard\\picard.exe";

        public const string SECTION = ToolsConfiguration.SECTION;

        public const string ENABLED_ELEMENT = "AAAA9D6F-71B7-4C80-AA30-F952576F378A";

        public const string PATH_ELEMENT = "BBBBD1CC-224C-4879-AC04-76B9FA3E98BC";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "Tools")
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled", path: "MusicBrainz Picard").WithValue(File.Exists(PATH)))
                .WithElement(new TextConfigurationElement(PATH_ELEMENT, "Path", path: "MusicBrainz Picard").WithValue(PATH).WithFlags(ConfigurationElementFlags.FileName)
            );
            StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(SECTION, ENABLED_ELEMENT).ConnectValue(value => UpdateConfiguration(value));
        }

        private static void UpdateConfiguration(bool enabled)
        {
            if (enabled)
            {
                StandardComponents.Instance.Configuration.GetElement(SECTION, PATH_ELEMENT).Show();
            }
            else
            {
                StandardComponents.Instance.Configuration.GetElement(SECTION, PATH_ELEMENT).Hide();
            }
        }
    }
}
