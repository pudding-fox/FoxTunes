using System.Collections.Generic;
using System.Windows.Controls;

namespace FoxTunes
{
    public static class PeakMeterBehaviourConfiguration
    {
        public const string SECTION = "EA1C25CD-795A-4D94-880F-602906B82BC1";

        public const string ORIENTATION = "AAAAC0C6-E7E3-4EEA-B34C-FC7E267A5CCA";

        public const string ORIENTATION_HORIZONTAL = "AAAAA9C7-C99D-4C8C-82A8-D50ED943DACF";

        public const string ORIENTATION_VERTICAL = "BBBBAE97-69EA-4722-B1F7-1BF6E33622B8";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.PeakMeterBehaviourConfiguration_Section)
                .WithElement(new SelectionConfigurationElement(ORIENTATION, Strings.PeakMeterBehaviourConfiguration_Orientation).WithOptions(GetOrientationOptions())
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetOrientationOptions()
        {
            yield return new SelectionConfigurationOption(ORIENTATION_HORIZONTAL, Strings.PeakMeterBehaviourConfiguration_Orientation_Horizontal);
            yield return new SelectionConfigurationOption(ORIENTATION_VERTICAL, Strings.PeakMeterBehaviourConfiguration_Orientation_Vertical);
        }

        public static Orientation GetOrientation(SelectionConfigurationOption option)
        {
            switch (option.Id)
            {
                default:
                case ORIENTATION_HORIZONTAL:
                    return Orientation.Horizontal;
                case ORIENTATION_VERTICAL:
                    return Orientation.Vertical;
            }
        }
    }
}
