using System.Collections.Generic;

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

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, Strings.SpectrogramBehaviourConfiguration_Mode, path: Strings.SpectrogramBehaviourConfiguration_Path).WithOptions(GetModeOptions())
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
    }
}
