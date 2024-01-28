using ManagedBass.Sox;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassResamplerStreamComponentConfiguration
    {
        public const string ENABLED_ELEMENT = "AAAA5C85-178C-470D-A977-C54350875AB3";

        public const string QUALITY_ELEMENT = "BBBB0ED2-67E6-4155-AFB0-FE8D7E7B9B8C";

        public const string PHASE_ELEMENT = "CCCC4941-E1DC-4C2E-9DF7-14C1638C392F";

        public const string STEEP_FILTER_ELEMENT = "DDDD0064-06D7-4EFA-B3C9-2820B8893039";

        public const string ALLOW_ALIASING_ELEMENT = "EEEE7228-F94E-4ADA-88AA-85F65CBC006A";

        public const string BUFFER_LENGTH_ELEMENT = "FFFF63F8-2695-4853-89E8-ADC10D8EFC2E";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION, "Output")
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled", path: "Resampler").WithValue(false))
                .WithElement(new SelectionConfigurationElement(QUALITY_ELEMENT, "Quality", path: "Resampler").WithOptions(GetQualityOptions()).DependsOn(BassOutputConfiguration.SECTION, ENABLED_ELEMENT))
                .WithElement(new SelectionConfigurationElement(PHASE_ELEMENT, "Phase", path: "Resampler").WithOptions(GetPhaseOptions()).DependsOn(BassOutputConfiguration.SECTION, ENABLED_ELEMENT))
                .WithElement(new BooleanConfigurationElement(STEEP_FILTER_ELEMENT, "Steep Filter", path: "Resampler").WithValue(false).DependsOn(BassOutputConfiguration.SECTION, ENABLED_ELEMENT))
                .WithElement(new BooleanConfigurationElement(ALLOW_ALIASING_ELEMENT, "Allow Aliasing", path: "Resampler").WithValue(false).DependsOn(BassOutputConfiguration.SECTION, ENABLED_ELEMENT))
                .WithElement(new IntegerConfigurationElement(BUFFER_LENGTH_ELEMENT, "Buffer Length", path: "Resampler").WithValue(3).WithValidationRule(new IntegerValidationRule(3, 10)).DependsOn(BassOutputConfiguration.SECTION, ENABLED_ELEMENT)
            );
        }

        public static SoxChannelQuality GetQuality(SelectionConfigurationOption option)
        {
            var quality = default(SoxChannelQuality);
            if (!Enum.TryParse<SoxChannelQuality>(option.Id, out quality))
            {
                return SoxChannelQuality.VeryHigh;
            }
            return quality;
        }

        private static IEnumerable<SelectionConfigurationOption> GetQualityOptions()
        {
            var values = new[]
            {
                SoxChannelQuality.Quick,
                SoxChannelQuality.Low,
                SoxChannelQuality.Medium,
                SoxChannelQuality.High,
                SoxChannelQuality.VeryHigh
            };
            foreach (var value in values)
            {
                var option = new SelectionConfigurationOption(
                    Enum.GetName(typeof(SoxChannelQuality), value),
                    Enum.GetName(typeof(SoxChannelQuality), value)
                );
                if (SoxChannelQuality.VeryHigh.Equals(value))
                {
                    option.Default();
                }
                yield return option;
            }
        }

        public static SoxChannelPhase GetPhase(SelectionConfigurationOption option)
        {
            var quality = default(SoxChannelPhase);
            if (!Enum.TryParse<SoxChannelPhase>(option.Id, out quality))
            {
                return SoxChannelPhase.Linear;
            }
            return quality;
        }

        private static IEnumerable<SelectionConfigurationOption> GetPhaseOptions()
        {
            var values = Enum.GetValues(typeof(SoxChannelPhase));
            foreach (var value in values)
            {
                var option = new SelectionConfigurationOption(
                    Enum.GetName(typeof(SoxChannelPhase), value),
                    Enum.GetName(typeof(SoxChannelPhase), value)
                );
                if (SoxChannelPhase.Linear.Equals(value))
                {
                    option.Default();
                }
                yield return option;
            }
        }
    }
}
