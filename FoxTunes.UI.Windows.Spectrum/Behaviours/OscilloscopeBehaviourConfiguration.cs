using System.Collections.Generic;

namespace FoxTunes
{
    public static class OscilloscopeBehaviourConfiguration
    {
        public const string SECTION = "A8627871-2D42-4129-874F-E067EE4DB1BD";

        public const string MODE_ELEMENT = "AAAAD149-F777-46CB-92C0-F479CEE72A91";

        public const string MODE_MONO_OPTION = "AAAA5CBA-52E7-47AB-98B6-AE2A937A4971";

        //TODO: This was never implemented.
        public const string MODE_STEREO_OPTION = "BBBB73E1-D8A5-42C4-809A-853A278E0170";

        public const string MODE_SEPERATE_OPTION = "CCCCD496-E88A-4B42-8A89-75FFB9A1CD49";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.OscilloscopeBehaviourConfiguration_Section)
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, Strings.OscilloscopeBehaviourConfiguration_Mode).WithOptions(GetModeOptions())
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetModeOptions()
        {
            yield return new SelectionConfigurationOption(MODE_MONO_OPTION, Strings.OscilloscopeBehaviourConfiguration_Mode_Mono).Default();
            yield return new SelectionConfigurationOption(MODE_SEPERATE_OPTION, Strings.OscilloscopeBehaviourConfiguration_Mode_Seperate);
        }

        public static OscilloscopeRendererMode GetMode(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case MODE_MONO_OPTION:
                    return OscilloscopeRendererMode.Mono;
                case MODE_SEPERATE_OPTION:
                    return OscilloscopeRendererMode.Seperate;
            }
        }
    }
}
