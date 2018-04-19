using FoxTunes.Interfaces;
using ManagedBass;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static class BassDirectSoundDevice
    {
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
            384000
        };

        static BassDirectSoundDevice()
        {
            Devices = new Dictionary<int, BassDirectSoundDeviceInfo>();
        }

        private static IDictionary<int, BassDirectSoundDeviceInfo> Devices { get; set; }

        public static int Device { get; private set; }

        public static bool IsInitialized { get; private set; }

        public static void Init()
        {
            IsInitialized = true;
            var info = default(DeviceInfo);
            BassUtils.OK(Bass.GetDeviceInfo(Device, out info));
            Devices[Device] = new BassDirectSoundDeviceInfo(
                info.Name,
                Bass.Info.SampleRate,
                0,
                Bass.Info.SpeakerCount,
                RATES.Where(rate => rate >= Bass.Info.MinSampleRate && rate <= Bass.Info.MaxSampleRate).ToArray()
            );
            LogManager.Logger.Write(typeof(BassDirectSoundDevice), LogLevel.Debug, "Detected DS device: {0} => Name => {1}, Inputs => {2}, Outputs = {3}, Rate = {4}", Device, Info.Name, Info.Inputs, Info.Outputs, Info.Rate);
            LogManager.Logger.Write(typeof(BassDirectSoundDevice), LogLevel.Debug, "Detected DS device: {0} => Rates => {1}", Device, string.Join(", ", Info.SupportedRates));
        }

        public static void Free()
        {
            if (!IsInitialized)
            {
                return;
            }
            IsInitialized = false;
        }

        public static BassDirectSoundDeviceInfo Info
        {
            get
            {
                return Devices[Device];
            }
        }

        public class BassDirectSoundDeviceInfo
        {
            public BassDirectSoundDeviceInfo(string name, int rate, int inputs, int outputs, IEnumerable<int> supportedRates)
            {
                this.Name = name;
                this.Rate = rate;
                this.Inputs = inputs;
                this.Outputs = outputs;
                this.SupportedRates = supportedRates;
            }

            public string Name { get; private set; }

            public int Rate { get; private set; }

            public int Inputs { get; private set; }

            public int Outputs { get; private set; }

            public IEnumerable<int> SupportedRates { get; private set; }
        }
    }
}
