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

        public const string DB_ELEMENT = "AAAA7F57-6B6B-4A51-A75F-83F4735CE464";

        public const string SMOOTHING_ELEMENT = "BBBBEDC6-A831-4B89-9501-D162F3E08B32";

        public const string COLOR_PALETTE_ELEMENT = "ZZZZ579D-F617-41CD-9007-6B0402BBE1AD";

        public const string COLOR_PALETTE_LOW = "LOW";

        public const string COLOR_PALETTE_MID = "MID";

        public const string COLOR_PALETTE_HIGH = "HIGH";

        public const string COLOR_PALETTE_BACKGROUND = "BACKGROUND";

        public const int SMOOTHING_MIN = 0;

        public const int SMOOTHING_LOW = 9;

        public const int SMOOTHING_MEDIUM = 15;

        public const int SMOOTHING_MAX = 30;

        public const int SMOOTHING_DEFAULT = 0;

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.BandedWaveFormStreamPositionConfiguration_Section)
                .WithElement(new BooleanConfigurationElement(DB_ELEMENT, Strings.BandedWaveFormStreamPositionConfiguration_DB).WithValue(false))
                .WithElement(new IntegerConfigurationElement(SMOOTHING_ELEMENT, Strings.BandedWaveFormStreamPositionConfiguration_Smoothing).WithValue(SMOOTHING_DEFAULT).WithValidationRule(new IntegerValidationRule(SMOOTHING_MIN, SMOOTHING_MAX, 3)))
                .WithElement(new TextConfigurationElement(COLOR_PALETTE_ELEMENT, Strings.BandedWaveFormStreamPositionConfiguration_ColorPalette).WithValue(GetDefaultColorPalette()).WithFlags(ConfigurationElementFlags.MultiLine)
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
                pair => pair.Key,
                pair => pair.Value.ToGradient(),
                StringComparer.OrdinalIgnoreCase
            );
        }
    }
}
