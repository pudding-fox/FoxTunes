using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace FoxTunes
{
    public static class BandedWaveFormStreamPositionConfiguration
    {
        public const string SECTION = "17D020D2-B9E9-400D-98EB-B730F1E7BBB5";

        public const string COLOR_PALETTE_ELEMENT = "AAAA579D-F617-41CD-9007-6B0402BBE1AD";

        public const string COLOR_PALETTE_THEME = "THEME";

        public const string COLOR_PALETTE_LOW = "LOW";

        public const string COLOR_PALETTE_MID = "MID";

        public const string COLOR_PALETTE_HIGH = "HIGH";

        public const string COLOR_PALETTE_VALUE = "VALUE";

        public const string COLOR_PALETTE_BACKGROUND = "BACKGROUND";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.BandedWaveFormStreamPositionConfiguration_Section)
                .WithElement(new TextConfigurationElement(COLOR_PALETTE_ELEMENT, Strings.BandedWaveFormStreamPositionConfiguration_ColorPalette).WithValue(GetDefaultColorPalette()).WithFlags(ConfigurationElementFlags.MultiLine)
            );
        }

        public static string GetDefaultColorPalette()
        {
            var builder = new StringBuilder();
            return builder.ToString();
        }

        public static IDictionary<string, Color[]> GetColorPalette(string value, Color[] colors)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    var palettes = value.ToNamedColorStops().ToDictionary(
                        pair => string.IsNullOrEmpty(pair.Key) ? COLOR_PALETTE_VALUE : pair.Key,
                        pair => pair.Value.ToGradient(),
                        StringComparer.OrdinalIgnoreCase
                    );
                    palettes[COLOR_PALETTE_THEME] = colors;
                    return palettes;
                }
                catch
                {
                    //Nothing can be done.
                }
            }
            return new Dictionary<string, Color[]>(StringComparer.OrdinalIgnoreCase)
            {
                { COLOR_PALETTE_THEME, colors }
            };
        }
    }
}
