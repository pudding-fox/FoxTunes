using System.Collections.Generic;

namespace FoxTunes
{
    public static class TagLibFileFactoryConfiguration
    {
        public const string READ_WINDOWS_MEDIA_TAGS = "JJKK0057-52AD-4914-BF4A-692EF76C7C83";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(MetaDataBehaviourConfiguration.SECTION, "Meta Data")
                .WithElement(new BooleanConfigurationElement(READ_WINDOWS_MEDIA_TAGS, "Windows Media Tags").WithValue(false)
            );
            StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT).ConnectValue(value => UpdateConfiguration(value));
        }

        private static void UpdateConfiguration(bool enabled)
        {
            if (enabled)
            {
                StandardComponents.Instance.Configuration.GetElement(MetaDataBehaviourConfiguration.SECTION, READ_WINDOWS_MEDIA_TAGS).Show();
            }
            else
            {
                StandardComponents.Instance.Configuration.GetElement(MetaDataBehaviourConfiguration.SECTION, READ_WINDOWS_MEDIA_TAGS).Hide();
            }
        }
    }
}
