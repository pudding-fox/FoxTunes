using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace FoxTunes
{
    public static class WaveBarBehaviourConfiguration
    {
        public const string SECTION = "1355A5DB-84D3-4582-B294-5B6EEC8B80D2";

        public const string MODE_ELEMENT = "402DE29B-50AB-4345-89FB-174D87824F98";

        public const string MODE_MONO_OPTION = "AAAA21C9-A9FF-4CFD-8EFB-96EE79455050";

        //TODO: This was never implemented.
        public const string MODE_STEREO_OPTION = "BBBB6F8B-5FCD-4EEC-997E-D3DF8B63BD36";

        public const string MODE_SEPERATE_OPTION = "CCCC3286-5097-4977-A4CC-5CEDA5E2E099";

        public const string RESOLUTION_ELEMENT = "AAAACCC0-596C-489C-BD39-E74C0AE3697C";

        public const string RMS_ELEMENT = "ABBB7F57-6B6B-4A51-A75F-83F4735CE464";

        public const string CACHE_ELEMENT = "BBBBAD4B-8BB4-47C9-9D18-2121C48115CE";

        public const string COLOR_PALETTE_ELEMENT = "CCCC7E5A-ECA1-4D45-92C1-82B9EF4F8228";

        public const string CLEANUP_ELEMENT = "ZZZZ8740-576A-4456-A2CC-0B1E7DEF6913";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.WaveBarBehaviourConfiguration_Section)
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, Strings.WaveBarBehaviourConfiguration_Mode).WithOptions(GetModeOptions()))
                .WithElement(new IntegerConfigurationElement(RESOLUTION_ELEMENT, Strings.WaveBarBehaviourConfiguration_Resolution, path: Strings.General_Advanced).WithValue(10).WithValidationRule(new IntegerValidationRule(1, 100)))
                .WithElement(new BooleanConfigurationElement(RMS_ELEMENT, Strings.WaveBarBehaviourConfiguration_RMS).WithValue(true))
                .WithElement(new BooleanConfigurationElement(CACHE_ELEMENT, Strings.WaveBarBehaviourConfiguration_Cache, path: Strings.General_Advanced).WithValue(true))
                .WithElement(new TextConfigurationElement(COLOR_PALETTE_ELEMENT, Strings.WaveBarBehaviourConfiguration_ColorPalette).WithValue(GetDefaultColorPalette()).WithFlags(ConfigurationElementFlags.MultiLine))
                .WithElement(new CommandConfigurationElement(CLEANUP_ELEMENT, Strings.WaveBarBehaviourConfiguration_Cleanup, path: Strings.General_Advanced)
                    .WithHandler(() => WaveFormCache.Cleanup())
                    .DependsOn(SECTION, CACHE_ELEMENT)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetModeOptions()
        {
            yield return new SelectionConfigurationOption(MODE_MONO_OPTION, Strings.WaveBarBehaviourConfiguration_Mode_Mono).Default();
            yield return new SelectionConfigurationOption(MODE_SEPERATE_OPTION, Strings.WaveBarBehaviourConfiguration_Mode_Seperate);
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

        public static Color[] GetColorPalette(string value, Color color)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    return value.ToPalette().MirrorGradient();
                }
                catch
                {
                    //Nothing can be done.
                }
            }
            return new[] { color };
        }
    }
}
