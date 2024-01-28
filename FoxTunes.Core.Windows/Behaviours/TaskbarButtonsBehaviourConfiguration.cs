using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class TaskbarButtonsBehaviourConfiguration
    {
        public const string SECTION = WindowsConfiguration.SECTION;

        public const string ENABLED_ELEMENT = "AAAA5AF6-A76C-4FB9-B783-ECB772AE1E54";

        public const string THUMBNAIL_ELEMENT = "CCCC32FB-CC7A-43C7-8C3D-525DDDBD12AA";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
            yield return new ConfigurationSection(SECTION, Strings.WindowsConfiguration_Section)
                .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, Strings.TaskbarButtonsBehaviourConfiguration_Enabled).WithValue(releaseType == ReleaseType.Default))
                .WithElement(new BooleanConfigurationElement(THUMBNAIL_ELEMENT, Strings.TaskbarThumbnailBehaviourConfiguration_Enabled).WithValue(true).DependsOn(SECTION, ENABLED_ELEMENT)
            );
        }
    }
}
