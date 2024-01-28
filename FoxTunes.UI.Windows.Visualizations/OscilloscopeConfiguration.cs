using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class OscilloscopeConfiguration
    {
        public const string SECTION = VisualizationBehaviourConfiguration.SECTION;

        public const string MODE_ELEMENT = "AAAAD149-F777-46CB-92C0-F479CEE72A91";

        public const string MODE_MONO_OPTION = "AAAA5CBA-52E7-47AB-98B6-AE2A937A4971";

        //TODO: This was never implemented.
        public const string MODE_STEREO_OPTION = "BBBB73E1-D8A5-42C4-809A-853A278E0170";

        public const string MODE_SEPERATE_OPTION = "CCCCD496-E88A-4B42-8A89-75FFB9A1CD49";

        public const string WINDOW_ELEMENT = "AABBF71F-72AD-46F6-964F-A4D30539C87E";

        public const int WINDOW_MIN = 100;

        public const int WINDOW_MAX = 500;

        public const int WINDOW_DEFAULT = 100;

        public const string DURATION_ELEMENT = "BBBBE8CC-88E9-4B66-B2FB-3577CD32D8C7";

        public const int DURATION_MIN = 100;

        public const int DURATION_MAX = 2000;

        public const int DURATION_DEFAULT = 400;

        public const string DROP_SHADOW_ELEMENT = "CCCCFC1C-70A0-4F9C-8EB4-37D59760CEC0";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, Strings.OscilloscopeConfiguration_Mode, path: Strings.OscilloscopeConfiguration_Path).WithOptions(GetModeOptions()))
                .WithElement(new IntegerConfigurationElement(WINDOW_ELEMENT, Strings.OscilloscopeConfiguration_Window, path: Strings.OscilloscopeConfiguration_Path).WithValue(WINDOW_DEFAULT).WithValidationRule(new IntegerValidationRule(WINDOW_MIN, WINDOW_MAX, 10)))
                .WithElement(new IntegerConfigurationElement(DURATION_ELEMENT, Strings.OscilloscopeConfiguration_Duration, path: Strings.OscilloscopeConfiguration_Path).WithValue(DURATION_DEFAULT).WithValidationRule(new IntegerValidationRule(DURATION_MIN, DURATION_MAX, 10)))
                .WithElement(new BooleanConfigurationElement(DROP_SHADOW_ELEMENT, Strings.OscilloscopeConfiguration_DropShadow, path: Strings.OscilloscopeConfiguration_Path).WithValue(false)
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetModeOptions()
        {
            yield return new SelectionConfigurationOption(MODE_MONO_OPTION, Strings.OscilloscopeConfiguration_Mode_Mono).Default();
            yield return new SelectionConfigurationOption(MODE_SEPERATE_OPTION, Strings.OscilloscopeConfiguration_Mode_Seperate);
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

        public static TimeSpan GetWindow(int value)
        {
            return TimeSpan.FromMilliseconds(value);
        }

        public static TimeSpan GetDuration(int value)
        {
            return TimeSpan.FromMilliseconds(value);
        }
    }
}
