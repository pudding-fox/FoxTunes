#if NET40

//ListView grouping is too slow under net40 due to lack of virtualization.

#else

using System.Collections.Generic;

namespace FoxTunes
{
    public static class PlaylistGroupingBehaviourConfiguration
    {
        public const string GROUP_ENABLED_ELEMENT = "BBA65418-FF22-486D-AFF4-85F2F738A852";

        public const string GROUP_SCRIPT_ELEMENT = "CE8245BF-9BE2-4853-9E2F-40389293EFED";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(PlaylistBehaviourConfiguration.SECTION, "Playlist")
                .WithElement(
                    new BooleanConfigurationElement(GROUP_ENABLED_ELEMENT, "Grouping").WithValue(false))
                .WithElement(
                    new TextConfigurationElement(GROUP_SCRIPT_ELEMENT, "Group Script", path: "Advanced").WithValue(Resources.Grouping).WithFlags(ConfigurationElementFlags.MultiLine)
            );
            StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(PlaylistBehaviourConfiguration.SECTION, GROUP_ENABLED_ELEMENT).ConnectValue(value => UpdateConfiguration(value));
        }

        private static void UpdateConfiguration(bool enabled)
        {
            if (enabled)
            {
                StandardComponents.Instance.Configuration.GetElement(PlaylistBehaviourConfiguration.SECTION, GROUP_SCRIPT_ELEMENT).Show();
            }
            else
            {
                StandardComponents.Instance.Configuration.GetElement(PlaylistBehaviourConfiguration.SECTION, GROUP_SCRIPT_ELEMENT).Hide();
            }
        }
    }
}

#endif
