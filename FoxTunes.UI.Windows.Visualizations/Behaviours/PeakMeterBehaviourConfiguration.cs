using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class PeakMeterBehaviourConfiguration
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public const string SECTION = VisualizationBehaviourConfiguration.SECTION;

        public const string PEAKS = "BBBBFFA3-8F13-4838-9429-69C5E208963D";

        public const string RMS = "CCCC0961-0743-4761-B27D-3ACEFA6EAC3C";

        public const string HOLD = "DDDDB7C2-7CBC-4728-B41C-2C310701F4AB";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION)
                .WithElement(new BooleanConfigurationElement(PEAKS, Strings.SpectrumBehaviourConfiguration_Peaks, path: string.Format("{0}/{1}", Strings.PeakMeterBehaviourConfiguration_Path, Strings.General_Advanced)))
                .WithElement(new BooleanConfigurationElement(RMS, Strings.EnhancedSpectrumBehaviourConfiguration_Rms, path: string.Format("{0}/{1}", Strings.PeakMeterBehaviourConfiguration_Path, Strings.General_Advanced)))
                .WithElement(new IntegerConfigurationElement(HOLD, Strings.SpectrumBehaviourConfiguration_Hold, path: string.Format("{0}/{1}", Strings.PeakMeterBehaviourConfiguration_Path, Strings.General_Advanced)).WithValue(1000).WithValidationRule(new IntegerValidationRule(500, 5000)).DependsOn(SECTION, PEAKS)
            );
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<SelectionConfigurationElement>(
                SECTION,
                VisualizationBehaviourConfiguration.QUALITY_ELEMENT
            ).ConnectValue(option => UpdateConfiguration(option));
        }

        private static void UpdateConfiguration(SelectionConfigurationOption option)
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            var peaks = configuration.GetElement<BooleanConfigurationElement>(
                SECTION,
                PEAKS
            );
            var rms = configuration.GetElement<BooleanConfigurationElement>(
                SECTION,
                RMS
            );
            switch (option.Id)
            {
                default:
                case VisualizationBehaviourConfiguration.QUALITY_HIGH_OPTION:
                    Logger.Write(typeof(SpectrumBehaviourConfiguration), LogLevel.Debug, "Using high quality profile.");
                    peaks.Value = true;
                    rms.Value = true;
                    break;
                case VisualizationBehaviourConfiguration.QUALITY_LOW_OPTION:
                    Logger.Write(typeof(SpectrumBehaviourConfiguration), LogLevel.Debug, "Using low quality profile.");
                    peaks.Value = false;
                    rms.Value = false;
                    break;
            }
        }
    }
}
