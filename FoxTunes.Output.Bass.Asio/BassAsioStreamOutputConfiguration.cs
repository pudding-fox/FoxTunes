using FoxTunes.Interfaces;
using ManagedBass.Asio;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassAsioStreamOutputConfiguration
    {
        public const int ASIO_NO_DEVICE = -1;

        public const string ELEMENT_ASIO_DEVICE = "2E20B9CE-96FC-4FBB-8956-84B9A7E3FEB3";

        public const string DSD_RAW_ELEMENT = "9044043A-8A30-42A0-B2CB-3DE379636DD6";

        public const string MODE_ASIO_OPTION = "598987DA-EE55-467A-B2F5-61480F2F12F6";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.OUTPUT_SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(BassOutputConfiguration.MODE_ELEMENT, "Mode")
                    .WithOption(new SelectionConfigurationOption(MODE_ASIO_OPTION, "ASIO")))
                .WithElement(new SelectionConfigurationElement(ELEMENT_ASIO_DEVICE, "Device")
                    .WithOptions(() => GetASIODevices()))
                .WithElement(new BooleanConfigurationElement(DSD_RAW_ELEMENT, "DSD Direct").WithValue(false));
            StandardComponents.Instance.Configuration.GetElement(BassOutputConfiguration.OUTPUT_SECTION, BassOutputConfiguration.MODE_ELEMENT).ConnectValue<string>(mode => UpdateConfiguration(mode));
        }

        public static int GetAsioDevice(string value)
        {
            for (var a = 0; a < BassAsio.DeviceCount; a++)
            {
                var deviceInfo = default(AsioDeviceInfo);
                BassAsioUtils.OK(BassAsio.GetDeviceInfo(a, out deviceInfo));
                if (string.Equals(deviceInfo.Name, value, StringComparison.OrdinalIgnoreCase))
                {
                    return a;
                }
            }
            return ASIO_NO_DEVICE;
        }

        private static IEnumerable<SelectionConfigurationOption> GetASIODevices()
        {
            for (var a = 0; a < BassAsio.DeviceCount; a++)
            {
                var deviceInfo = default(AsioDeviceInfo);
                BassAsioUtils.OK(BassAsio.GetDeviceInfo(a, out deviceInfo));
                LogManager.Logger.Write(typeof(BassAsioStreamOutputConfiguration), LogLevel.Debug, "ASIO Device: {0} => {1} => {2}", a, deviceInfo.Name, deviceInfo.Driver);
                yield return new SelectionConfigurationOption(deviceInfo.Name, deviceInfo.Name, deviceInfo.Driver);
            }
        }

        private static void UpdateConfiguration(string mode)
        {
            switch (mode)
            {
                case MODE_ASIO_OPTION:
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, ELEMENT_ASIO_DEVICE).Show();
                    StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, DSD_RAW_ELEMENT).Show();
                    break;
                default:
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, ELEMENT_ASIO_DEVICE).Hide();
                    StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, DSD_RAW_ELEMENT).Hide();
                    break;
            }
        }
    }
}
