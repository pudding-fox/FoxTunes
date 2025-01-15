using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace FoxTunes
{
    public static class MoodBarStreamPositionConfiguration
    {
        public const string SECTION = "4029E5F6-83E5-4DB9-9A22-666E051AF565";

        public const string COLOR_PALETTE_ELEMENT = "ZZZZ9011-940E-473E-94C2-54449FD28ECA";

        public const string COLOR_PALETTE_LOW = "LOW";

        public const string COLOR_PALETTE_MID = "MID";

        public const string COLOR_PALETTE_HIGH = "HIGH";

        public const string COLOR_PALETTE_VALUE = "VALUE";

        public const string COLOR_PALETTE_BACKGROUND = "BACKGROUND";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.MoodBarStreamPositionConfiguration_Section)
                .WithElement(new TextConfigurationElement(COLOR_PALETTE_ELEMENT, Strings.MoodBarStreamPositionConfiguration_ColorPalette).WithValue(GetDefaultColorPalette()).WithFlags(ConfigurationElementFlags.MultiLine)
            );
        }

        public static string GetDefaultColorPalette()
        {
            var builder = new StringBuilder();
            return builder.ToString();
        }

        public static IDictionary<string, Color[]> GetColorPalette(string value)
        {
            return value.ToNamedColorStops().ToDictionary(
                pair => string.IsNullOrEmpty(pair.Key) ? COLOR_PALETTE_VALUE : pair.Key,
                pair => pair.Value.ToGradient(),
                StringComparer.OrdinalIgnoreCase
            );
        }
    }
}
