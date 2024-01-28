using FoxTunes.Interfaces;
using ManagedBass.Wasapi;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassWasapiStreamOutputConfiguration
    {
        public const string ELEMENT_WASAPI_DEVICE = "AAAA9BD4-C90C-46A8-A486-2EBAE7152051";

        public const string MODE_WASAPI_OPTION = "BBBB737C-0B07-4842-A59A-03679E1716F3";

        public const string ELEMENT_WASAPI_EXCLUSIVE = "CCCC20A2-3A5B-4F64-A574-89561114AAF4";

        public const string ELEMENT_WASAPI_EVENT = "DDDD16CF-03A5-4DDC-BE23-2C619D21F447";

        public const string ELEMENT_WASAPI_DITHER = "EEEEBF20-7B2A-489F-9FBD-E6FE5458F6B5";

        public const string MIXER_ELEMENT = "FFFF34F9-BB72-4DB6-BDD0-F5C9BFD2F9EE";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(BassOutputConfiguration.SECTION, "Output")
                .WithElement(new SelectionConfigurationElement(BassOutputConfiguration.MODE_ELEMENT, "Mode")
                    .WithOptions(new[] { new SelectionConfigurationOption(MODE_WASAPI_OPTION, "WASAPI") }))
                .WithElement(new SelectionConfigurationElement(ELEMENT_WASAPI_DEVICE, "Device", path: "WASAPI")
                    .WithOptions(GetWASAPIDevices()))
                .WithElement(new BooleanConfigurationElement(ELEMENT_WASAPI_EXCLUSIVE, "Exclusive", path: "WASAPI").WithValue(true))
                .WithElement(new BooleanConfigurationElement(ELEMENT_WASAPI_EVENT, "Event", path: "WASAPI").WithValue(false))
                .WithElement(new BooleanConfigurationElement(ELEMENT_WASAPI_DITHER, "Dither", path: "WASAPI").WithValue(false))
                .WithElement(new BooleanConfigurationElement(MIXER_ELEMENT, "Mixer", path: "WASAPI").WithValue(false)
            );
        }

        public static int GetWasapiDevice(SelectionConfigurationOption option)
        {
            if (!string.Equals(option.Id, BassWasapi.DefaultDevice.ToString()))
            {
                for (int a = 0, b = BassWasapi.DeviceCount; a < b; a++)
                {
                    var deviceInfo = default(WasapiDeviceInfo);
                    BassUtils.OK(BassWasapi.GetDeviceInfo(a, out deviceInfo));
                    if (string.Equals(deviceInfo.ID, option.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        return a;
                    }
                }
            }
            return BassWasapi.DefaultDevice;
        }

        private static IEnumerable<SelectionConfigurationOption> GetWASAPIDevices()
        {
            yield return new SelectionConfigurationOption(BassWasapi.DefaultDevice.ToString(), "Default Device");
            for (int a = 0, b = BassWasapi.DeviceCount; a < b; a++)
            {
                var deviceInfo = default(WasapiDeviceInfo);
                BassUtils.OK(BassWasapi.GetDeviceInfo(a, out deviceInfo));
                if (deviceInfo.IsInput || deviceInfo.IsDisabled || deviceInfo.IsLoopback || deviceInfo.IsUnplugged)
                {
                    continue;
                }
                LogManager.Logger.Write(typeof(BassWasapiStreamOutputConfiguration), LogLevel.Debug, "WASAPI Device: {0} => {1} => {2} => {3} => {4}", a, deviceInfo.ID, deviceInfo.Name, Enum.GetName(typeof(WasapiDeviceType), deviceInfo.Type), deviceInfo.MixFrequency);
                yield return new SelectionConfigurationOption(deviceInfo.ID, deviceInfo.Name, string.Format("{0} ({1})", deviceInfo.Name, Enum.GetName(typeof(WasapiDeviceType), deviceInfo.Type)));
            }
        }
    }
}
