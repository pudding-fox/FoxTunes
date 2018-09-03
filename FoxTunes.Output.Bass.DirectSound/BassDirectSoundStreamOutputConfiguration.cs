using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassDirectSoundStreamOutputConfiguration
    {
        public const string MODE_DS_OPTION = "F8691348-069B-4763-89CF-5ACBE53E9F75";

        public const string ELEMENT_DS_DEVICE = "CBF8D4A5-4DD5-4985-A373-565335084B80";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.OUTPUT_SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(BassOutputConfiguration.MODE_ELEMENT, "Mode")
                    .WithOption(new SelectionConfigurationOption(MODE_DS_OPTION, "Direct Sound"), true))
                .WithElement(new SelectionConfigurationElement(ELEMENT_DS_DEVICE, "Device")
                    .WithOptions(() => GetDSDevices()));
            StandardComponents.Instance.Configuration.GetElement(BassOutputConfiguration.OUTPUT_SECTION, BassOutputConfiguration.MODE_ELEMENT).ConnectValue<string>(mode => UpdateConfiguration(mode));
        }

        public static int GetDsDevice(string value)
        {
            if (!string.Equals(value, Bass.DefaultDevice.ToString()))
            {
                for (int a = 0, b = Bass.DeviceCount; a < b; a++)
                {
                    var deviceInfo = default(DeviceInfo);
                    BassUtils.OK(Bass.GetDeviceInfo(a, out deviceInfo));
                    if (string.Equals(deviceInfo.Name, value, StringComparison.OrdinalIgnoreCase))
                    {
                        return a;
                    }
                }
            }
            return Bass.DefaultDevice;
        }

        private static IEnumerable<SelectionConfigurationOption> GetDSDevices()
        {
            yield return new SelectionConfigurationOption(Bass.DefaultDevice.ToString(), "Default Device").Default();
            for (int a = 0, b = Bass.DeviceCount; a < b; a++)
            {
                var deviceInfo = default(DeviceInfo);
                BassUtils.OK(Bass.GetDeviceInfo(a, out deviceInfo));
                LogManager.Logger.Write(typeof(BassDirectSoundStreamOutputConfiguration), LogLevel.Debug, "DS Device: {0} => {1} => {2}", a, deviceInfo.Name, deviceInfo.Driver);
                if (!deviceInfo.IsEnabled)
                {
                    continue;
                }
                yield return new SelectionConfigurationOption(deviceInfo.Name, deviceInfo.Name, deviceInfo.Driver);
            }
        }

        private static void UpdateConfiguration(string mode)
        {
            switch (mode)
            {
                case MODE_DS_OPTION:
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, ELEMENT_DS_DEVICE).Show();
                    break;
                default:
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, ELEMENT_DS_DEVICE).Hide();
                    break;
            }
        }
    }
}
