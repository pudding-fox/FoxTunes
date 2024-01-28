using FoxTunes.Interfaces;
using ManagedBass;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassDirectSoundDevice
    {
        public static bool IsInitialized { get; private set; }

        public static bool IsDefaultDevice
        {
            get
            {
                return Info != null && Info.Device == Bass.DefaultDevice;
            }
        }

        public static void Init(int device)
        {
            IsInitialized = true;
            var info = default(DeviceInfo);
            BassUtils.OK(Bass.GetDeviceInfo(BassUtils.GetDeviceNumber(device), out info));
            Info = new BassDirectSoundDeviceInfo(
                device,
                0,
                Bass.Info.SpeakerCount,
                OutputRate.GetRates(Bass.Info.MinSampleRate, Bass.Info.MaxSampleRate)
            );
            LogManager.Logger.Write(typeof(BassDirectSoundDevice), LogLevel.Debug, "Detected DS device: {0} => Inputs => {1}, Outputs = {2}, Rate = {3}", device, Info.Inputs, Info.Outputs, Info.Rate);
            LogManager.Logger.Write(typeof(BassDirectSoundDevice), LogLevel.Debug, "Detected DS device: {0} => Rates => {1}", device, string.Join(", ", Info.SupportedRates));
        }

        public static void Free()
        {
            if (!IsInitialized)
            {
                return;
            }
            IsInitialized = false;
        }

        public static BassDirectSoundDeviceInfo Info { get; private set; }

        public class BassDirectSoundDeviceInfo
        {
            public BassDirectSoundDeviceInfo(int device, int inputs, int outputs, IEnumerable<int> supportedRates)
            {
                this.Device = device;
                this.Inputs = inputs;
                this.Outputs = outputs;
                this.SupportedRates = supportedRates;
            }

            public int Device { get; private set; }

            public int Rate
            {
                get
                {
                    return Bass.Info.SampleRate;
                }
            }

            public int Inputs { get; private set; }

            public int Outputs { get; private set; }

            public IEnumerable<int> SupportedRates { get; private set; }
        }
    }
}
