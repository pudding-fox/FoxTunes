using FoxTunes.Interfaces;
using ManagedBass.Asio;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassAsioStreamOutputConfiguration
    {
        public const int ASIO_NO_DEVICE = -1;

        public const string ELEMENT_ASIO_DEVICE = "AAAAB9CE-96FC-4FBB-8956-84B9A7E3FEB3";

        public const string DSD_RAW_ELEMENT = "BBBB043A-8A30-42A0-B2CB-3DE379636DD6";

        public const string MODE_ASIO_OPTION = "CCCC87DA-EE55-467A-B2F5-61480F2F12F6";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(BassOutputConfiguration.MODE_ELEMENT, "Mode")
                    .WithOptions(new[] { new SelectionConfigurationOption(MODE_ASIO_OPTION, "ASIO") }))
                .WithElement(new SelectionConfigurationElement(ELEMENT_ASIO_DEVICE, "Device", path: "ASIO")
                    .WithOptions(GetASIODevices()))
                .WithElement(new BooleanConfigurationElement(DSD_RAW_ELEMENT, "DSD Direct", path: "ASIO").WithValue(false));
        }

        public static int GetAsioDevice(SelectionConfigurationOption option)
        {
            for (int a = 0, b = BassAsio.DeviceCount; a < b; a++)
            {
                var deviceInfo = default(AsioDeviceInfo);
                BassAsioUtils.OK(BassAsio.GetDeviceInfo(a, out deviceInfo));
                if (string.Equals(deviceInfo.Name, option.Id, StringComparison.OrdinalIgnoreCase))
                {
                    return a;
                }
            }
            return ASIO_NO_DEVICE;
        }

        private static IEnumerable<SelectionConfigurationOption> GetASIODevices()
        {
            for (int a = 0, b = BassAsio.DeviceCount; a < b; a++)
            {
                var deviceInfo = default(AsioDeviceInfo);
                BassAsioUtils.OK(BassAsio.GetDeviceInfo(a, out deviceInfo));
                LogManager.Logger.Write(typeof(BassAsioStreamOutputConfiguration), LogLevel.Debug, "ASIO Device: {0} => {1} => {2}", a, deviceInfo.Name, deviceInfo.Driver);
                yield return new SelectionConfigurationOption(deviceInfo.Name, deviceInfo.Name, deviceInfo.Driver);
            }
        }
    }
}
