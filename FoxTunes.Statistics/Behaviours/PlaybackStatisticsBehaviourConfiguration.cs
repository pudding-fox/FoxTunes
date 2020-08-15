using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class PlaybackStatisticsBehaviourConfiguration
    {
        public const string ENABLED = "2B879758-2672-4513-A348-FDD08E1E8500";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            yield return new ConfigurationSection(PlaybackBehaviourConfiguration.SECTION, "Playback")
                .WithElement(
                    new BooleanConfigurationElement(ENABLED, "Update Play Counters").WithValue(releaseType == ReleaseType.Default)
            );
        }
    }
}
