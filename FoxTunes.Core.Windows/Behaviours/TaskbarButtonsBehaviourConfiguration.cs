using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class TaskbarButtonsBehaviourConfiguration
    {
        public const string SECTION = "8CCE4686-B497-489E-8BC3-274DC8BDCBCB";

        public const string ENABLED_ELEMENT = "AAAA5AF6-A76C-4FB9-B783-ECB772AE1E54";

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
                yield return new ConfigurationSection(SECTION, "Taskbar Buttons")
                    .WithElement(new BooleanConfigurationElement(ENABLED_ELEMENT, "Enabled").WithValue(false)
                );
            }
        }
    }
}
