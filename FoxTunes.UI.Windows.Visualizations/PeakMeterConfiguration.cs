using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace FoxTunes
{
    public static class PeakMeterConfiguration
    {
        public const string SECTION = VisualizationBehaviourConfiguration.SECTION;

        public const string PEAKS = "BBBBFFA3-8F13-4838-9429-69C5E208963D";

        public const string RMS = "CCCC0961-0743-4761-B27D-3ACEFA6EAC3C";

        public const string COLOR_PALETTE = "EEEE1493-AF31-4450-A39B-396014866DDF";

        public const string COLOR_PALETTE_PEAK = "PEAK";

        public const string COLOR_PALETTE_RMS = "RMS";

        public const string COLOR_PALETTE_VALUE = "VALUE";

        public const string COLOR_PALETTE_BACKGROUND = "BACKGROUND";

        public const string DURATION = "FFFF6E54-7313-45D9-9E30-6D7205725365";

        public const int DURATION_MIN = 16;

        public const int DURATION_MAX = 64;

        public const int DURATION_DEFAULT = 32;

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.PeakMeterConfiguration_Section)
                .WithElement(new BooleanConfigurationElement(PEAKS, Strings.PeakMeterConfiguration_Peaks).WithValue(true))
                .WithElement(new BooleanConfigurationElement(RMS, Strings.PeakMeterConfiguration_Rms).WithValue(false))
                .WithElement(new TextConfigurationElement(COLOR_PALETTE, Strings.PeakMeterConfiguration_ColorPalette, path: Strings.General_Advanced).WithValue(GetDefaultColorPalette()).WithFlags(ConfigurationElementFlags.MultiLine))
                .WithElement(new IntegerConfigurationElement(DURATION, Strings.PeakMeterConfiguration_Duration).WithValue(DURATION_DEFAULT).WithValidationRule(new IntegerValidationRule(DURATION_MIN, DURATION_MAX)))
                .WithElement(new IntegerConfigurationElement(VisualizationBehaviourConfiguration.INTERVAL_ELEMENT, Strings.VisualizationBehaviourConfiguration_Interval, path: Strings.General_Advanced).WithValue(VisualizationBehaviourConfiguration.DEFAULT_INTERVAL).WithValidationRule(new IntegerValidationRule(VisualizationBehaviourConfiguration.MIN_INTERVAL, VisualizationBehaviourConfiguration.MAX_INTERVAL))
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
