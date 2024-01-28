using FoxTunes.Interfaces;
using ManagedBass.Wasapi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace FoxTunes
{
    public static class BassWasapiDevice
    {
        const int INIT_ATTEMPTS = 5;

        const int INIT_ATTEMPT_INTERVAL = 400;

        public const int PRIMARY_CHANNEL = 0;

        public const int SECONDARY_CHANNEL = 1;

        public static int[] RATES = new[]
        {
            //PCM
            44100,
            48000,
            88200,
            96000,
            176400,
            192000,
            352800,
            384000,
            //DSD - There are variations of each, for some reason.
            2822400, //DSD64,
            3072000, //DSD64
            5644800, //DSD128
            6144000,  //DSD128
            11289600, //DSD256
            12288000, //DSD256
            22579200, //DSD512
            24576000, //DSD512
        };

        static BassWasapiDevice()
        {
            Devices = new Dictionary<int, BassWasapiDeviceInfo>();
        }

        private static IDictionary<int, BassWasapiDeviceInfo> Devices { get; set; }

        public static int Device { get; private set; }

        public static bool IsDefaultDevice
        {
            get
            {
                return Device == BassWasapi.DefaultDevice;
            }
        }

        public static bool Exclusive { get; private set; }

        public static bool EventDriven { get; private set; }

        public static bool IsInitialized { get; private set; }

        public static void Init(int frequency = 0, int channels = 0)
        {
            Init(Device, Exclusive, EventDriven, frequency, channels);
        }

        public static void Init(int device, bool exclusive, bool eventDriven, int frequency = 0, int channels = 0)
        {
            LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Debug, "Initializing BASS WASAPI.");
            var flags = WasapiInitFlags.Shared;
            if (exclusive)
            {
                flags |= WasapiInitFlags.Exclusive;
            }
            if (eventDriven)
            {
                flags |= WasapiInitFlags.EventDriven;
            }
            BassUtils.OK(BassWasapiHandler.Init(device, frequency, channels, flags, 0, 0));
            IsInitialized = true;
            Device = device;
            Exclusive = exclusive;
            EventDriven = eventDriven;
            var exception = default(Exception);
            for (var a = 1; a <= INIT_ATTEMPTS; a++)
            {
                LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Debug, "Detecting WASAPI device, attempt: {0}", a);
                try
                {
                    Devices[device] = new BassWasapiDeviceInfo(
                        BassWasapi.Info.Frequency,
                        0,
                        BassWasapi.Info.Channels,
                        GetSupportedFormats(device, flags),
                        BassWasapi.Info.Format
                    );
                    LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Debug, "Detected WASAPI device: {0} => Inputs => {1}, Outputs = {2}, Rate = {3}, Format = {4}", Device, Info.Inputs, Info.Outputs, Info.Rate, Enum.GetName(typeof(WasapiFormat), Info.Format));
                    LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Debug, "Detected WASAPI device: {0} => Rates => {1}", Device, string.Join(", ", Info.SupportedRates));
                    return;
                }
                catch (Exception e)
                {
                    exception = e;
                    LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Warn, "Failed to detect WASAPI device: {0}", e.Message);
                }
                Thread.Sleep(INIT_ATTEMPT_INTERVAL);
            }
            if (exception != null)
            {
                Free();
                throw exception;
            }
            throw new NotImplementedException();
        }

        private static IDictionary<int, WasapiFormat> GetSupportedFormats(int device, WasapiInitFlags flags)
        {
            var supportedFormats = new Dictionary<int, WasapiFormat>();
            foreach (var rate in RATES)
            {
                var format = BassWasapi.CheckFormat(device, rate, BassWasapi.Info.Channels, flags);
                if (format == WasapiFormat.Unknown)
                {
                    continue;
                }
                supportedFormats.Add(rate, format);
            }
            return supportedFormats;
        }

        public static void Free()
        {
            if (!IsInitialized)
            {
                return;
            }
            LogManager.Logger.Write(typeof(BassWasapiDevice), LogLevel.Debug, "Releasing BASS WASAPI.");
            BassWasapi.Free();
            BassWasapiHandler.Free();
            IsInitialized = false;
        }

        public static BassWasapiDeviceInfo Info
        {
            get
            {
                if (!Devices.ContainsKey(Device))
                {
                    return null;
                }
                return Devices[Device];
            }
        }

        public class BassWasapiDeviceInfo
        {
            public BassWasapiDeviceInfo(int rate, int inputs, int outputs, IDictionary<int, WasapiFormat> supportedFormats, WasapiFormat format)
            {
                this.Rate = rate;
                this.Inputs = inputs;
                this.Outputs = outputs;
#if NET40
                this.SupportedFormats = new Dictionary<int, WasapiFormat>(supportedFormats);
#else
                this.SupportedFormats = new ReadOnlyDictionary<int, WasapiFormat>(supportedFormats);
#endif
                this.Format = format;
            }

            public int Rate { get; private set; }

            public int Inputs { get; private set; }

            public int Outputs { get; private set; }

            public IEnumerable<int> SupportedRates
            {
                get
                {
                    return this.SupportedFormats.Keys;
                }
            }

#if NET40
            public Dictionary<int, WasapiFormat> SupportedFormats { get; private set; }
#else
            public IReadOnlyDictionary<int, WasapiFormat> SupportedFormats { get; private set; }
#endif

            public WasapiFormat Format { get; private set; }
        }
    }
}
