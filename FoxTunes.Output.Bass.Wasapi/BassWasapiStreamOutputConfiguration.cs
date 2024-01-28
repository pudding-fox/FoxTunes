using FoxTunes.Interfaces;
using ManagedBass.Wasapi;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassWasapiStreamOutputConfiguration
    {
        public const string ELEMENT_WASAPI_DEVICE = "C6C59BD4-C90C-46A8-A486-2EBAE7152051";

        public const string MODE_WASAPI_OPTION = "336F737C-0B07-4842-A59A-03679E1716F3";

        public const string ELEMENT_WASAPI_EXCLUSIVE = "72AE20A2-3A5B-4F64-A574-89561114AAF4";

        public const string ELEMENT_WASAPI_EVENT = "2D1F16CF-03A5-4DDC-BE23-2C619D21F447";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.OUTPUT_SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(BassOutputConfiguration.MODE_ELEMENT, "Mode")
                    .WithOption(new SelectionConfigurationOption(MODE_WASAPI_OPTION, "WASAPI")))
                .WithElement(new SelectionConfigurationElement(ELEMENT_WASAPI_DEVICE, "Device")
                    .WithOptions(() => GetWASAPIDevices()))
                .WithElement(new BooleanConfigurationElement(ELEMENT_WASAPI_EXCLUSIVE, "Exclusive").WithValue(false))
                .WithElement(new BooleanConfigurationElement(ELEMENT_WASAPI_EVENT, "Event").WithValue(false));
            StandardComponents.Instance.Configuration.GetElement(BassOutputConfiguration.OUTPUT_SECTION, BassOutputConfiguration.MODE_ELEMENT).ConnectValue<string>(mode => UpdateConfiguration(mode));
        }

        public static int GetWasapiDevice(string value)
        {
            for (int a = 0, b = BassWasapi.DeviceCount; a < b; a++)
            {
                var deviceInfo = default(WasapiDeviceInfo);
                BassUtils.OK(BassWasapi.GetDeviceInfo(a, out deviceInfo));
                if (string.Equals(deviceInfo.ID, value, StringComparison.OrdinalIgnoreCase))
                {
                    return a;
                }
            }
            return BassWasapi.DefaultDevice;
        }

        private static IEnumerable<SelectionConfigurationOption> GetWASAPIDevices()
        {
            yield return new SelectionConfigurationOption(BassWasapi.DefaultDevice.ToString(), "Default Device").Default();
            for (int a = 0, b = BassWasapi.DeviceCount; a < b; a++)
            {
                var deviceInfo = default(WasapiDeviceInfo);
                BassUtils.OK(BassWasapi.GetDeviceInfo(a, out deviceInfo));
                if (deviceInfo.IsInput || deviceInfo.IsDisabled || deviceInfo.IsLoopback || deviceInfo.IsUnplugged)
                {
                    continue;
                }
                LogManager.Logger.Write(typeof(BassWasapiStreamOutputConfiguration), LogLevel.Debug, "WASAPI Device: {0} => {1} => {2} => {3}", a, deviceInfo.ID, deviceInfo.Name, Enum.GetName(typeof(WasapiDeviceType), deviceInfo.Type));
                yield return new SelectionConfigurationOption(deviceInfo.ID, deviceInfo.Name, string.Format("{0} ({1})", deviceInfo.Name, Enum.GetName(typeof(WasapiDeviceType), deviceInfo.Type)));
            }
        }

        private static void UpdateConfiguration(string mode)
        {
            switch (mode)
            {
                case MODE_WASAPI_OPTION:
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, ELEMENT_WASAPI_DEVICE).Show();
                    StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, ELEMENT_WASAPI_EXCLUSIVE).Show();
                    StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, ELEMENT_WASAPI_EVENT).Show();
                    break;
                default:
                    StandardComponents.Instance.Configuration.GetElement<SelectionConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, ELEMENT_WASAPI_DEVICE).Hide();
                    StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, ELEMENT_WASAPI_EXCLUSIVE).Hide();
                    StandardComponents.Instance.Configuration.GetElement<BooleanConfigurationElement>(BassOutputConfiguration.OUTPUT_SECTION, ELEMENT_WASAPI_EVENT).Hide();
                    break;
            }
        }
    }
}
