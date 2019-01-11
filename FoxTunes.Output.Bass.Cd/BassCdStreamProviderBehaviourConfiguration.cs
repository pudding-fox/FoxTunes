using FoxTunes.Interfaces;
using ManagedBass.Cd;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassCdStreamProviderBehaviourConfiguration
    {
        public const int CD_NO_DRIVE = -1;

        public const string SECTION = "220BF762-28B1-436C-951D-5B0359473A40";

        public const string DRIVE_ELEMENT = "99BD557F-62E7-4152-88DF-BF90C7C661F5";

        public const string LOOKUP_ELEMENT = "4C1F29AB-ED22-4AB2-AD79-0EFE4EAB39B7";

        public const string LOOKUP_HOST_ELEMENT = "D1D587EE-07E2-4B95-8F9D-039738956A30";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, "CD")
                .WithElement(new SelectionConfigurationElement(DRIVE_ELEMENT, "Drive")
                    .WithOptions(() => GetDrives()))
                .WithElement(new BooleanConfigurationElement(LOOKUP_ELEMENT, "Lookup Tags")
                    .WithValue(true))
                .WithElement(new TextConfigurationElement(LOOKUP_HOST_ELEMENT, "Host")
                    .WithValue(BassCd.CDDBServer));
            StandardComponents.Instance.Configuration.GetElement(SECTION, LOOKUP_ELEMENT).ConnectValue<bool>(value => UpdateConfiguration(value));
        }

        private static IEnumerable<SelectionConfigurationOption> GetDrives()
        {
            for (var a = 0; a < BassCd.DriveCount; a++)
            {
                var cdInfo = default(CDInfo);
                BassUtils.OK(BassCd.GetInfo(a, out cdInfo));
                LogManager.Logger.Write(typeof(BassCdStreamProviderBehaviourConfiguration), LogLevel.Debug, "CD Drive: {0} => {1} => {2}", a, cdInfo.Name, cdInfo.Manufacturer);
                yield return new SelectionConfigurationOption(cdInfo.Name, string.Format("{0} ({1}:\\)", cdInfo.Name, cdInfo.DriveLetter), cdInfo.Name);
            }
        }

        public static int GetDrive(string value)
        {
            for (var a = 0; a < BassCd.DriveCount; a++)
            {
                var cdInfo = default(CDInfo);
                BassUtils.OK(BassCd.GetInfo(a, out cdInfo));
                if (string.Equals(cdInfo.Name, value, StringComparison.OrdinalIgnoreCase))
                {
                    return a;
                }
            }
            return CD_NO_DRIVE;
        }

        private static void UpdateConfiguration(bool cdda)
        {
            if (cdda)
            {
                StandardComponents.Instance.Configuration.GetElement<TextConfigurationElement>(SECTION, LOOKUP_HOST_ELEMENT).Show();
            }
            else
            {
                StandardComponents.Instance.Configuration.GetElement<TextConfigurationElement>(SECTION, LOOKUP_HOST_ELEMENT).Hide();
            }
        }
    }
}
