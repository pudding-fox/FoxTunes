using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class TaskbarButtonsBehaviourConfiguration
    {
        public const string SECTION = "8CCE4686-B497-489E-8BC3-274DC8BDCBCB";

        public const string ENABLED_ELEMENT = "AAAA5AF6-A76C-4FB9-B783-ECB772AE1E54";

        public const string PROGRESS_ELEMENT = "BBBB676E-ACEA-4D5F-8B1D-B02758CAE959";

        public const string THUMBNAIL_ELEMENT = "CCCC32FB-CC7A-43C7-8C3D-525DDDBD12AA";

        public static bool IsPlatformSupported
        {
            get
            {
                if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1)
                {
                    //Windows 7.
                    return true;
                }
                if (Environment.OSVersion.Version.Major > 6)
                {
                    //Windows 8 or greater.
                    return true;
                }
                return false;
            }
        }

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            if (IsPlatformSupported)
            {
                var releaseType = StandardComponents.Instance.Configuration.ReleaseType;
                yield return new ConfigurationSection(SECTION, Strings.TaskbarButtonsBehaviourConfiguration_Section)
                    .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, Strings.TaskbarButtonsBehaviourConfiguration_Enabled).WithValue(releaseType == ReleaseType.Default))
                    .WithElement(new BooleanConfigurationElement(PROGRESS_ELEMENT, Strings.TaskbarButtonsBehaviourConfiguration_Progress).WithValue(true).DependsOn(SECTION, ENABLED_ELEMENT))
                    .WithElement(new BooleanConfigurationElement(THUMBNAIL_ELEMENT, Strings.TaskbarButtonsBehaviourConfiguration_Thumbnail).WithValue(true).DependsOn(SECTION, ENABLED_ELEMENT)
                );
            }
        }
    }
}
