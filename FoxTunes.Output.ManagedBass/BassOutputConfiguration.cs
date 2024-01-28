using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Asio;
using ManagedBass.Wasapi;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassOutputConfiguration
    {
        public const int ASIO_NO_DEVICE = -1;

        public const int WASAPI_NO_DEVICE = -1;

        public const string OUTPUT_SECTION = "8399D051-81BC-41A6-940D-36E6764213D2";

        public const string RATE_ELEMENT = "0452A558-F1ED-41B1-A3DC-95158E01003C";

        public const string RATE_044100_OPTION = "12616570-DDC5-4455-96F2-1D2625EAEC0C";

        public const string RATE_048000_OPTION = "4E96C885-E3F5-425E-9699-5EDE705B9720";

        public const string RATE_088200_OPTION = "4C5ACEDF-7A76-499F-A60B-25B9C6B1BE52";

        public const string RATE_096000_OPTION = "0D5F7382-7F31-4971-9605-FB2B23EA8954";

        public const string RATE_176400_OPTION = "D0CCFBF1-A5D4-4B9F-AD08-2E6E70DEBBCF";

        public const string RATE_192000_OPTION = "3F00BE95-307B-4B80-B8DA-40798889945B";

        public const string DEPTH_ELEMENT = "F535A2A6-DCA0-4E27-9812-498BB2A2C4BC";

        public const string DEPTH_16_OPTION = "768B8466-7582-4B1C-8687-7AB75D636CD8";

        public const string DEPTH_32_OPTION = "889573F2-2E08-4F2B-B94E-DAA945D96497";

        public const string ELEMENT_DS_DEVICE = "CBF8D4A5-4DD5-4985-A373-565335084B80";

        public const string ELEMENT_ASIO_DEVICE = "2E20B9CE-96FC-4FBB-8956-84B9A7E3FEB3";

        public const string ELEMENT_WASAPI_DEVICE = "ADBFBF5B-8BC8-44E8-8366-012E3CBA047D";

        public const string MODE_ELEMENT = "76096B39-2F8A-4667-9C03-9742FF2D1EA7";

        public const string MODE_DS_OPTION = "F8691348-069B-4763-89CF-5ACBE53E9F75";

        public const string MODE_ASIO_OPTION = "598987DA-EE55-467A-B2F5-61480F2F12F6";

        public const string MODE_WASAPI_OPTION = "1AB7AB7A-1186-4A98-B42E-C5C3A5581D62";

        public const string DSD_RAW_ELEMENT = "9044043A-8A30-42A0-B2CB-3DE379636DD6";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(OUTPUT_SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(RATE_ELEMENT, "Rate")
                    .WithOption(new SelectionConfigurationOption(RATE_044100_OPTION, "44100"), true)
                    .WithOption(new SelectionConfigurationOption(RATE_048000_OPTION, "48000"))
                    .WithOption(new SelectionConfigurationOption(RATE_088200_OPTION, "88200"))
                    .WithOption(new SelectionConfigurationOption(RATE_096000_OPTION, "96000"))
                    .WithOption(new SelectionConfigurationOption(RATE_176400_OPTION, "176400"))
                    .WithOption(new SelectionConfigurationOption(RATE_192000_OPTION, "192000")))
                .WithElement(new SelectionConfigurationElement(DEPTH_ELEMENT, "Depth")
                    .WithOption(new SelectionConfigurationOption(DEPTH_16_OPTION, "16bit"), true)
                    .WithOption(new SelectionConfigurationOption(DEPTH_32_OPTION, "32bit floating point")))
                .WithElement(new SelectionConfigurationElement(ELEMENT_DS_DEVICE, "Device")
                    .WithOptions(() => GetDSDevices()))
                .WithElement(new SelectionConfigurationElement(ELEMENT_ASIO_DEVICE, "Device")
                    .WithOptions(() => GetASIODevices()))
                .WithElement(new SelectionConfigurationElement(ELEMENT_WASAPI_DEVICE, "Device")
                    .WithOptions(() => GetWASAPIDevices()))
                .WithElement(new SelectionConfigurationElement(MODE_ELEMENT, "Mode")
                    .WithOption(new SelectionConfigurationOption(MODE_DS_OPTION, "Direct Sound"), true)
                    .WithOption(new SelectionConfigurationOption(MODE_ASIO_OPTION, "ASIO"))
                    .WithOption(new SelectionConfigurationOption(MODE_WASAPI_OPTION, "WASAPI")))
                .WithElement(new BooleanConfigurationElement(DSD_RAW_ELEMENT, "DSD Direct").WithValue(false)
            );
            StandardComponents.Instance.Configuration.GetElement(OUTPUT_SECTION, MODE_ELEMENT).ConnectValue<string>(mode => UpdateDevices(mode));
        }

        private static IEnumerable<SelectionConfigurationOption> GetDSDevices()
        {
            yield return new SelectionConfigurationOption(Bass.DefaultDevice.ToString(), "Default Device").Default();
            for (var a = 0; a < Bass.DeviceCount; a++)
            {
                var deviceInfo = default(DeviceInfo);
                BassUtils.OK(Bass.GetDeviceInfo(a, out deviceInfo));
                LogManager.Logger.Write(typeof(BassOutputConfiguration), LogLevel.Debug, "DS Device: {0} => {1} => {2}", a, deviceInfo.Name, deviceInfo.Driver);
                if (!deviceInfo.IsEnabled)
                {
                    continue;
                }
                yield return new SelectionConfigurationOption(deviceInfo.Name, deviceInfo.Name, deviceInfo.Driver);
            }
        }

        private static IEnumerable<SelectionConfigurationOption> GetASIODevices()
        {
            for (var a = 0; a < BassAsio.DeviceCount; a++)
            {
                var deviceInfo = default(AsioDeviceInfo);
                BassUtils.OK(BassAsio.GetDeviceInfo(a, out deviceInfo));
                LogManager.Logger.Write(typeof(BassOutputConfiguration), LogLevel.Debug, "ASIO Device: {0} => {1} => {2}", a, deviceInfo.Name, deviceInfo.Driver);
                yield return new SelectionConfigurationOption(deviceInfo.Name, deviceInfo.Name, deviceInfo.Driver);
            }
        }

        private static IEnumerable<SelectionConfigurationOption> GetWASAPIDevices()
        {
            for (var a = 0; a < BassWasapi.DeviceCount; a++)
            {
                var deviceInfo = default(WasapiDeviceInfo);
                BassUtils.OK(BassWasapi.GetDeviceInfo(a, out deviceInfo));
                if (!deviceInfo.IsEnabled || deviceInfo.IsUnplugged || deviceInfo.IsInput)
                {
                    continue;
                }
                LogManager.Logger.Write(typeof(BassOutputConfiguration), LogLevel.Debug, "WASAPI Device: {0} => {1} => {2}", a, deviceInfo.Name, deviceInfo.ID);
                var option = new SelectionConfigurationOption(deviceInfo.Name, deviceInfo.Name, deviceInfo.ID);
                if (deviceInfo.IsDefault)
                {
                    option.Default();
                }
                yield return option;
            }
        }

        private static void UpdateDevices(string mode)
        {
            switch (mode)
            {
                case MODE_DS_OPTION:
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(OUTPUT_SECTION, ELEMENT_DS_DEVICE).Show();
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(OUTPUT_SECTION, ELEMENT_ASIO_DEVICE).Hide();
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(OUTPUT_SECTION, ELEMENT_WASAPI_DEVICE).Hide();
                    StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(OUTPUT_SECTION, DSD_RAW_ELEMENT).Hide();
                    break;
                case MODE_ASIO_OPTION:
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(OUTPUT_SECTION, ELEMENT_DS_DEVICE).Hide();
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(OUTPUT_SECTION, ELEMENT_ASIO_DEVICE).Show();
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(OUTPUT_SECTION, ELEMENT_WASAPI_DEVICE).Hide();
                    StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(OUTPUT_SECTION, DSD_RAW_ELEMENT).Show();
                    break;
                case MODE_WASAPI_OPTION:
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(OUTPUT_SECTION, ELEMENT_DS_DEVICE).Hide();
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(OUTPUT_SECTION, ELEMENT_ASIO_DEVICE).Hide();
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(OUTPUT_SECTION, ELEMENT_WASAPI_DEVICE).Show();
                    StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(OUTPUT_SECTION, DSD_RAW_ELEMENT).Show();
                    break;
            }
        }

        public static int GetRate(string value)
        {
            switch (value)
            {
                default:
                case RATE_044100_OPTION:
                    return 44100;
                case RATE_048000_OPTION:
                    return 48000;
                case RATE_088200_OPTION:
                    return 88200;
                case RATE_096000_OPTION:
                    return 96000;
                case RATE_176400_OPTION:
                    return 176400;
                case RATE_192000_OPTION:
                    return 192000;
            }
        }

        public static bool GetFloat(string value)
        {
            switch (value)
            {
                default:
                case DEPTH_16_OPTION:
                    return false;
                case DEPTH_32_OPTION:
                    return true;
            }
        }

        public static BassOutputMode GetMode(string value)
        {
            switch (value)
            {
                default:
                case MODE_DS_OPTION:
                    return BassOutputMode.DirectSound;
                case MODE_ASIO_OPTION:
                    return BassOutputMode.ASIO;
                case MODE_WASAPI_OPTION:
                    return BassOutputMode.WASAPI;
            }
        }

        public static int GetDsDevice(string value)
        {
            if (!string.Equals(value, Bass.DefaultDevice.ToString()))
            {
                for (var a = 0; a < Bass.DeviceCount; a++)
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

        public static int GetAsioDevice(string value)
        {
            for (var a = 0; a < BassAsio.DeviceCount; a++)
            {
                var deviceInfo = default(AsioDeviceInfo);
                BassUtils.OK(BassAsio.GetDeviceInfo(a, out deviceInfo));
                if (string.Equals(deviceInfo.Name, value, StringComparison.OrdinalIgnoreCase))
                {
                    return a;
                }
            }
            return ASIO_NO_DEVICE;
        }

        public static int GetWasapiDevice(string value)
        {
            for (var a = 0; a < BassWasapi.DeviceCount; a++)
            {
                var deviceInfo = default(WasapiDeviceInfo);
                BassUtils.OK(BassWasapi.GetDeviceInfo(a, out deviceInfo));
                if (string.Equals(deviceInfo.Name, value, StringComparison.OrdinalIgnoreCase))
                {
                    return a;
                }
            }
            return WASAPI_NO_DEVICE;
        }
    }
}
