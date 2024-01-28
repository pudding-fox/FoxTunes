using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassDsdStreamProviderBehaviourConfiguration
    {
        public const string DSD_RATE_ELEMENT = "AAAAC170-6E90-4E55-9C14-8FCDAA39276B";

        public const string DSD_GAIN_ELEMENT = "BBBB4749-63C8-4866-9013-9AF01072CD2D";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(DSD_RATE_ELEMENT, "PCM Rate (Hz)", path: "DSD").WithOptions(GetRateOptions()))
                .WithElement(new IntegerConfigurationElement(DSD_GAIN_ELEMENT, "PCM Gain (dB)", path: "DSD").WithValue(6).WithValidationRule(new IntegerValidationRule(0, 10))
            );
        }

        public static IEnumerable<SelectionConfigurationOption> GetRateOptions()
        {
            foreach (var rate in OutputRate.PCM)
            {
                var option = new SelectionConfigurationOption(rate.ToString(), rate.ToString());
                if (rate == OutputRate.PCM_88200)
                {
                    option.Default();
                }
                yield return option;
            }
        }

        public static int GetRate(SelectionConfigurationOption option)
        {
            var rate = default(int);
            if (int.TryParse(option.Id, out rate))
            {
                return rate;
            }
            return OutputRate.PCM_88200;
        }
    }
}
