using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace FoxTunes
{
    public static class PeakMeterConfiguration
    {
        public const string SECTION = VisualizationBehaviourConfiguration.SECTION;

        public const string PEAKS = "BBBBFFA3-8F13-4838-9429-69C5E208963D";

        public const string RMS = "CCCC0961-0743-4761-B27D-3ACEFA6EAC3C";

        public const int MIN_HOLD = 500;

        public const int MAX_HOLD = 5000;

        public const int DEFAULT_HOLD = 1000;

        public const string HOLD = "DDDDB7C2-7CBC-4728-B41C-2C310701F4AB";

        public const string COLOR_PALETTE = "EEEE1493-AF31-4450-A39B-396014866DDF";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new BooleanConfigurationElement(PEAKS, Strings.PeakMeterConfiguration_Peaks, path: string.Format("{0}/{1}", Strings.PeakMeterConfiguration_Path, Strings.General_Advanced)).WithValue(true))
                .WithElement(new BooleanConfigurationElement(RMS, Strings.PeakMeterConfiguration_Rms, path: string.Format("{0}/{1}", Strings.PeakMeterConfiguration_Path, Strings.General_Advanced)).WithValue(true))
                .WithElement(new IntegerConfigurationElement(HOLD, Strings.PeakMeterConfiguration_Hold, path: string.Format("{0}/{1}", Strings.PeakMeterConfiguration_Path, Strings.General_Advanced)).WithValue(DEFAULT_HOLD).WithValidationRule(new IntegerValidationRule(MIN_HOLD, MAX_HOLD)).DependsOn(SECTION, PEAKS))
                .WithElement(new TextConfigurationElement(COLOR_PALETTE, Strings.PeakMeterConfiguration_ColorPalette, path: string.Format("{0}/{1}", Strings.PeakMeterConfiguration_Path, Strings.General_Advanced)).WithValue(GetDefaultColorPalette()).WithFlags(ConfigurationElementFlags.MultiLine).DependsOn(SECTION, RMS, true)
            );
        }

        public static string GetDefaultColorPalette()
        {
            var builder = new StringBuilder();
            return builder.ToString();
        }

        public static Color[] GetColorPalette(string value, Color[] colors)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    return value.ToColorStops().ToGradient();
                }
                catch
                {
                    //Nothing can be done.
                }
            }
            return colors;
        }
    }
}
