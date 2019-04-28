using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassEncoderSettingsFactoryConfiguration
    {
        private static IDictionary<string, IBassEncoderSettings> Profiles = GetProfiles();

        private static IDictionary<string, IBassEncoderSettings> GetProfiles()
        {
            var profiles = new Dictionary<string, IBassEncoderSettings>();
            foreach (var profile in BassEncoderSettingsFactory.Profiles)
            {
                var type = profile.GetType();
                var name = type.Name;
                var attribute = type.GetCustomAttribute<BassEncoderAttribute>();
                if (attribute != null)
                {
                    name = attribute.Name;
                }
                profiles.Add(name, profile);
            }
            return profiles;
        }

        public const string SECTION = BassEncoderBehaviourConfiguration.SECTION;

        public const string FORMAT_ELEMENT = "AAAA7464-2172-4E22-A3F6-817888878A62";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var section = new ConfigurationSection(SECTION, "Converter")
                .WithElement(new SelectionConfigurationElement(FORMAT_ELEMENT, "Format")
                    .WithOptions(GetFormatOptions())
            );
            foreach (var profile in BassEncoderSettingsFactory.Profiles)
            {
                foreach (var element in profile.GetConfigurationElements())
                {
                    section.WithElement(element);
                }
            }
            yield return section;
        }

        private static IEnumerable<SelectionConfigurationOption> GetFormatOptions()
        {
            foreach (var key in Profiles.Keys)
            {
                yield return new SelectionConfigurationOption(key, key);
            }
        }

        public static IBassEncoderSettings GetFormat(SelectionConfigurationOption option)
        {
            var profile = default(IBassEncoderSettings);
            if (!Profiles.TryGetValue(option.Id, out profile))
            {
                throw new NotImplementedException();
            }
            return profile;
        }
    }
}
