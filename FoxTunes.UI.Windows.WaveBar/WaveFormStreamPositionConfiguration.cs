using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace FoxTunes
{
    public static class WaveFormStreamPositionConfiguration
    {
        public const string SECTION = "1355A5DB-84D3-4582-B294-5B6EEC8B80D2";

        public const string MODE_ELEMENT = "402DE29B-50AB-4345-89FB-174D87824F98";

        public const string MODE_MONO_OPTION = "AAAA21C9-A9FF-4CFD-8EFB-96EE79455050";

        //TODO: This was never implemented.
        public const string MODE_STEREO_OPTION = "BBBB6F8B-5FCD-4EEC-997E-D3DF8B63BD36";

        public const string MODE_SEPERATE_OPTION = "CCCC3286-5097-4977-A4CC-5CEDA5E2E099";

        public const string RMS_ELEMENT = "ABBB7F57-6B6B-4A51-A75F-83F4735CE464";

        public const string COLOR_PALETTE_ELEMENT = "CCCC7E5A-ECA1-4D45-92C1-82B9EF4F8228";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.WaveFormStreamPositionConfiguration_Section)
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, Strings.WaveFormStreamPositionConfiguration_Mode).WithOptions(GetModeOptions()))
                .WithElement(new BooleanConfigurationElement(RMS_ELEMENT, Strings.WaveFormStreamPositionConfiguration_RMS).WithValue(true))
                .WithElement(new TextConfigurationElement(COLOR_PALETTE_ELEMENT, Strings.WaveFormStreamPositionConfiguration_ColorPalette).WithValue(GetDefaultColorPalette()).WithFlags(ConfigurationElementFlags.MultiLine).DependsOn(SECTION, RMS_ELEMENT, true)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetModeOptions()
        {
            yield return new SelectionConfigurationOption(MODE_MONO_OPTION, Strings.WaveFormStreamPositionConfiguration_Mode_Mono).Default();
            yield return new SelectionConfigurationOption(MODE_SEPERATE_OPTION, Strings.WaveFormStreamPositionConfiguration_Mode_Seperate);
        }

        public static WaveFormRendererMode GetMode(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case MODE_MONO_OPTION:
                    return WaveFormRendererMode.Mono;
                case MODE_SEPERATE_OPTION:
                    return WaveFormRendererMode.Seperate;
            }
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
                    return value.ToColorStops().ToGradient().MirrorGradient(true);
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
