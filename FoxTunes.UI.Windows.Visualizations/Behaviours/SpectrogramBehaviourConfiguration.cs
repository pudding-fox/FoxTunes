using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace FoxTunes
{
    public static class SpectrogramBehaviourConfiguration
    {
        public const string SECTION = VisualizationBehaviourConfiguration.SECTION;

        public const string MODE_ELEMENT = "AAAA4D78-941A-42D3-A704-88E295D17324";

        public const string MODE_MONO_OPTION = "AAAABCAA-3545-4861-A4D0-F84475C09C6D";

        //TODO: This was never implemented.
        public const string MODE_STEREO_OPTION = "BBBB989E-0726-4E87-BFB3-A63D19D96D79";

        public const string MODE_SEPERATE_OPTION = "CCCC8D37-067D-4A43-8AE8-7AD000E3E176";

        public const string SCALE_ELEMENT = "BBBB3A79-02DD-4D56-A4F9-4641CF2BA1C8";

        public const string SCALE_LINEAR_OPTION = "AAAA4A52-A0E5-40CA-888B-3D6A29EC0683";

        public const string SCALE_LOGARITHMIC_OPTION = "BBBB5C59-8888-4734-A191-954709375BD0";

        public const string SMOOTHING_ELEMENT = "CCCC0B5F-C79E-410F-B302-704BF628DD57";

        public const int SMOOTHING_MIN = 0;

        public const int SMOOTHING_MAX = 5;

        public const int SMOOTHING_DEFAULT = 1;

        public const string COLOR_PALETTE_ELEMENT = "DDDDBAFC-0C99-4329-9DA2-C041E674597E";

        public const string HISTORY_ELEMENT = "EEEE2EBD-4483-4281-A4AD-E042E0367996";

        public const int HISTORY_MIN = 0;

        public const int HISTORY_MAX = 8192;

        public const int HISTORY_DEFAULT = 2048;

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, Strings.SpectrogramBehaviourConfiguration_Mode, path: Strings.SpectrogramBehaviourConfiguration_Path).WithOptions(GetModeOptions()))
                .WithElement(new SelectionConfigurationElement(SCALE_ELEMENT, Strings.SpectrogramBehaviourConfiguration_Scale, path: Strings.SpectrogramBehaviourConfiguration_Path).WithOptions(GetScaleOptions()))
                .WithElement(new IntegerConfigurationElement(SMOOTHING_ELEMENT, Strings.SpectrogramBehaviourConfiguration_Smoothing, path: Strings.SpectrogramBehaviourConfiguration_Path).WithValue(SMOOTHING_DEFAULT).WithValidationRule(new IntegerValidationRule(SMOOTHING_MIN, SMOOTHING_MAX)))
                .WithElement(new TextConfigurationElement(COLOR_PALETTE_ELEMENT, Strings.SpectrogramBehaviourConfiguration_ColorPalette, path: Strings.SpectrogramBehaviourConfiguration_Path).WithValue(GetDefaultColorPalette()).WithFlags(ConfigurationElementFlags.MultiLine))
                .WithElement(new IntegerConfigurationElement(HISTORY_ELEMENT, Strings.SpectrogramBehaviourConfiguration_History, path: Strings.SpectrogramBehaviourConfiguration_Path).WithValue(Publication.ReleaseType == ReleaseType.Default ? HISTORY_DEFAULT : HISTORY_MIN).WithValidationRule(new IntegerValidationRule(HISTORY_MIN, HISTORY_MAX))
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetModeOptions()
        {
            yield return new SelectionConfigurationOption(MODE_MONO_OPTION, Strings.SpectrogramBehaviourConfiguration_Mode_Mono).Default();
            yield return new SelectionConfigurationOption(MODE_SEPERATE_OPTION, Strings.SpectrogramBehaviourConfiguration_Mode_Seperate);
        }

        public static SpectrogramRendererMode GetMode(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case MODE_MONO_OPTION:
                    return SpectrogramRendererMode.Mono;
                case MODE_SEPERATE_OPTION:
                    return SpectrogramRendererMode.Seperate;
            }
        }

        private static IEnumerable<SelectionConfigurationOption> GetScaleOptions()
        {
            yield return new SelectionConfigurationOption(SCALE_LINEAR_OPTION, Strings.SpectrogramBehaviourConfiguration_Scale_Linear).Default();
            yield return new SelectionConfigurationOption(SCALE_LOGARITHMIC_OPTION, Strings.SpectrogramBehaviourConfiguration_Scale_Logarithmic);
        }

        public static SpectrogramRendererScale GetScale(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case SCALE_LINEAR_OPTION:
                    return SpectrogramRendererScale.Linear;
                case SCALE_LOGARITHMIC_OPTION:
                    return SpectrogramRendererScale.Logarithmic;
            }
        }

        public static string GetDefaultColorPalette()
        {
            var builder = new StringBuilder();
            builder.AppendLine("0:Black");
            builder.AppendLine("100:White");
            return builder.ToString();
        }

        public static Color[] GetColorPalette()
        {
            return GetColorPalette(GetDefaultColorPalette());
        }

        public static Color[] GetColorPalette(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return GetColorPalette();
            }
            var lines = value
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .ToArray();
            var colors = new List<KeyValuePair<int, Color>>();
            foreach (var line in lines)
            {
                var parts = line
                    .Split(new[] { ':' }, 2)
                    .Select(part => part.Trim())
                    .ToArray();
                if (parts.Length != 2)
                {
                    continue;
                }
                var index = default(int);
                if (!int.TryParse(parts[0], out index))
                {
                    index = colors.Count * 10;
                }
                var color = default(Color);
                try
                {
                    color = (Color)ColorConverter.ConvertFromString(parts[1]);
                }
                catch
                {
                    //Failed to parse the color.
                    color = Colors.Red;
                }
                colors.Add(new KeyValuePair<int, Color>(index, color));
            }
            if (colors.Count < 2)
            {
                //Need at least two colors.
                return GetColorPalette();
            }
            return colors
                .OrderBy(color => color.Key)
                .ToArray()
                .ToGradient();
        }
    }
}
