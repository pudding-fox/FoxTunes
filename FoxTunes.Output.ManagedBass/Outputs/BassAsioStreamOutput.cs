using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Asio;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace FoxTunes
{
    public class BassAsioStreamOutput : BassStreamOutput
    {
        static BassAsioStreamOutput()
        {
            Devices = new Dictionary<int, BassAsioDeviceInfo>();
        }

        protected static IDictionary<int, BassAsioDeviceInfo> Devices { get; private set; }

        public static void Init(IBassOutput output)
        {
            BassUtils.OK(Bass.Configure(Configuration.UpdateThreads, 0));
            BassUtils.OK(Bass.Init(Bass.NoSoundDevice));
            Logger.Write(typeof(BassAsioStreamOutput), LogLevel.Debug, "BASS (No Sound) Initialized.");
        }

        public static void Free()
        {
            //Nothing to do.
        }

        const int START_ATTEMPTS = 5;

        const int START_ATTEMPT_INTERVAL = 400;

        const int PRIMARY_CHANNEL = 0;

        const int SECONDARY_CHANNEL = 1;

        public BassAsioStreamOutput(int device, int rate, int channels, BassFlags flags)
        {
            this.Device = device;
            this.Rate = rate;
            this.Channels = channels;
            this.Flags = flags;
        }

        public override BassStreamOutputCapability Capabilities
        {
            get
            {
                return BassStreamOutputCapability.DSD;
            }
        }

        public int Device { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        protected virtual void AddOrUpdateDeviceInfo(int device)
        {
            var info = default(AsioChannelInfo);
            BassUtils.OK(BassAsio.ChannelGetInfo(false, PRIMARY_CHANNEL, out info));
            Devices[device] = new BassAsioDeviceInfo(info.Name, info.Format);
        }

        public override bool CheckFormat(int rate, int channels)
        {
            return BassAsio.CheckRate(rate) && channels <= BassAsio.Info.Outputs;
        }

        public override void Connect(IBassStreamComponent previous)
        {
            Logger.Write(this, LogLevel.Debug, "Initializing BASS ASIO.");
            BassUtils.OK(BassAsio.Init(this.Device, AsioInitFlags.Thread));
            BassUtils.OK(BassAsioHandler.Init());
            this.AddOrUpdateDeviceInfo(this.Device);
            if (this.Channels > BassAsio.Info.Outputs)
            {
                //TODO: We should down mix.
                Logger.Write(this, LogLevel.Error, "Cannot play stream with more channels than device outputs.");
                throw new NotImplementedException(string.Format("The stream contains {0} channels which is greater than {1} output channels provided by the device.", this.Channels, BassAsio.Info.Outputs));
            }
            var success = BassUtils.OK(this.ConfigureASIO(previous));
            if (success)
            {
                if (previous.Flags.HasFlag(BassFlags.DSDRaw) || BassUtils.GetChannelDsdRaw(previous.ChannelHandle))
                {
                    success = BassUtils.OK(this.ConfigureASIO_DSD(previous));
                }
                else
                {
                    success = BassUtils.OK(this.ConfigureASIO_PCM(previous));
                }
            }
            if (!success)
            {
                throw new NotImplementedException("The device does not support the specified format.");
            }
        }

        protected virtual bool StartASIO()
        {
            for (var a = 1; a <= START_ATTEMPTS; a++)
            {
                Logger.Write(this, LogLevel.Debug, "Starting ASIO, attempt: {0}", a);
                try
                {
                    var success = BassAsio.Start(BassAsio.Info.PreferredBufferLength);
                    if (success)
                    {
                        Logger.Write(this, LogLevel.Debug, "Successfully started ASIO.");
                        return true;
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Warn, "Failed to start ASIO: {0}", Enum.GetName(typeof(Errors), BassAsio.LastError));
                    }
                }
                catch (Exception e)
                {
                    //Nothing can be done.
                    Logger.Write(this, LogLevel.Warn, "Failed to start ASIO: {0}", e.Message);
                }
                Thread.Sleep(START_ATTEMPT_INTERVAL);
            }
            Logger.Write(this, LogLevel.Warn, "Failed to start ASIO after {0} attempts.", START_ATTEMPTS);
            return false;
        }

        protected virtual bool StopASIO()
        {
            if (!BassAsio.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "ASIO has not been started.");
                return false;
            }
            Logger.Write(this, LogLevel.Debug, "Stopping ASIO.");
            BassUtils.OK(BassAsio.Stop());
            return true;
        }

        protected virtual bool ConfigureASIO(IBassStreamComponent previous)
        {
            BassUtils.OK(BassAsioHandler.StreamSet(previous.ChannelHandle));
            BassUtils.OK(BassAsioHandler.ChannelEnable(false, PRIMARY_CHANNEL));
            if (this.Channels == 1)
            {
                BassUtils.OK(BassAsio.ChannelEnableMirror(SECONDARY_CHANNEL, false, PRIMARY_CHANNEL));
            }
            else
            {
                for (var channel = 1; channel < this.Channels; channel++)
                {
                    BassUtils.OK(BassAsio.ChannelJoin(false, channel, PRIMARY_CHANNEL));
                }
            }
            BassUtils.OK(BassAsio.ChannelSetRate(false, PRIMARY_CHANNEL, previous.Rate));
            return true;
        }

        protected virtual bool ConfigureASIO_PCM(IBassStreamComponent previous)
        {
            Logger.Write(this, LogLevel.Debug, "Configuring PCM.");
            BassUtils.OK(BassAsio.SetDSD(false));
            if (!this.CheckFormat(this.Rate, this.Channels))
            {
                Logger.Write(this, LogLevel.Warn, "PCM format {0}:{1} is unsupported.", this.Rate, this.Channels);
                return false;
            }
            else
            {
                BassAsio.Rate = this.Rate;
            }
            if (previous.Flags.HasFlag(BassFlags.Float))
            {
                Logger.Write(this, LogLevel.Debug, "PCM: Rate = {0}, Format = {1}", BassAsio.Rate, Enum.GetName(typeof(AsioSampleFormat), AsioSampleFormat.Float));
                BassUtils.OK(BassAsio.ChannelSetFormat(false, PRIMARY_CHANNEL, AsioSampleFormat.Float));
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "PCM: Rate = {0}, Format = {1}", BassAsio.Rate, Enum.GetName(typeof(AsioSampleFormat), AsioSampleFormat.Bit16));
                BassUtils.OK(BassAsio.ChannelSetFormat(false, PRIMARY_CHANNEL, AsioSampleFormat.Bit16));
            }
            return true;
        }

        protected virtual bool ConfigureASIO_DSD(IBassStreamComponent previous)
        {
            try
            {
                Logger.Write(this, LogLevel.Debug, "Configuring DSD RAW.");
                try
                {
                    BassUtils.OK(BassAsio.SetDSD(true));
                }
                catch
                {
                    //If we get here some drivers (at least Creative) will crash when BassAsio.Start is called.
                    //I can't find a way to prevent it but it seems to be related to the allocated buffer size
                    //not being what the driver *thinks* it is and over-flowing.
                    //
                    //It should be unlikely in real life as the device would have to report capability of some
                    //very high PCM frequencies.
                    Logger.Write(this, LogLevel.Error, "Failed to enable DSD RAW on the device. Creative ASIO driver becomes unstable and usually crashes soon...");
                    return false;
                }
                if (!this.CheckFormat(this.Rate, this.Channels))
                {
                    Logger.Write(this, LogLevel.Warn, "DSD format {0}:{1} is unsupported.", this.Rate, this.Channels);
                    return false;
                }
                else
                {
                    BassAsio.Rate = this.Rate;
                }
                Logger.Write(this, LogLevel.Debug, "DSD: Rate = {0}, Format = {1}", BassAsio.Rate, Enum.GetName(typeof(AsioSampleFormat), AsioSampleFormat.DSD_MSB));
                BassUtils.OK(BassAsio.ChannelSetFormat(false, PRIMARY_CHANNEL, AsioSampleFormat.DSD_MSB));
                return true;
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to configure DSD RAW: {0}", e.Message);
                return false;
            }
        }

        protected virtual bool ResetASIO()
        {
            try
            {
                var flags =
                    AsioChannelResetFlags.Enable |
                    AsioChannelResetFlags.Join |
                    AsioChannelResetFlags.Format |
                    AsioChannelResetFlags.Rate;
                Logger.Write(this, LogLevel.Debug, "Resetting channel attributes.");
                for (var channel = 0; channel < BassAsio.Info.Outputs; channel++)
                {
                    BassUtils.OK(BassAsio.ChannelReset(false, channel, flags));
                }
            }
            catch (Exception e)
            {
                //Nothing can be done.
                Logger.Write(this, LogLevel.Warn, "Failed to reset channel attributes: {0}", e.Message);
            }
            return true;
        }

        public override bool IsPlaying
        {
            get
            {
                return BassAsio.IsStarted;
            }
        }

        public override bool IsPaused
        {
            get
            {
                return BassAsio.ChannelIsActive(false, PRIMARY_CHANNEL) == AsioChannelActive.Paused;
            }
        }

        public override bool IsStopped
        {
            get
            {
                return !BassAsio.IsStarted;
            }
        }

        public override int Latency
        {
            get
            {
                return BassAsio.GetLatency(false);
            }
        }

        public override void Play()
        {
            if (BassAsio.IsStarted)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Starting ASIO.");
            try
            {
                BassUtils.OK(this.StartASIO());
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public override void Pause()
        {
            Logger.Write(this, LogLevel.Debug, "Pausing ASIO.");
            try
            {
                BassUtils.OK(BassAsio.ChannelPause(false, PRIMARY_CHANNEL));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public override void Resume()
        {
            Logger.Write(this, LogLevel.Debug, "Resuming ASIO.");
            try
            {
                BassUtils.OK(BassAsio.ChannelReset(false, PRIMARY_CHANNEL, AsioChannelResetFlags.Pause));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public override void Stop()
        {
            if (!BassAsio.IsStarted)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Stopping ASIO.");
            try
            {
                BassUtils.OK(this.StopASIO());
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        protected override void OnDisposing()
        {
            if (BassAsio.IsStarted)
            {
                BassUtils.OK(this.StopASIO());
                BassUtils.OK(this.ResetASIO());
            }
            Logger.Write(this, LogLevel.Debug, "Releasing BASS ASIO.");
            BassUtils.OK(BassAsio.Free());
            BassUtils.OK(BassAsioHandler.Free());
        }

        protected class BassAsioDeviceInfo
        {
            public BassAsioDeviceInfo(string name, AsioSampleFormat format)
            {
                this.Name = name;
                this.Format = format;
            }

            public string Name { get; private set; }

            public AsioSampleFormat Format { get; private set; }
        }

        protected class BassAsioHandler
        {
            const string DllName = "bass_asio_handler";

            [DllImport(DllName)]
            static extern bool BASS_ASIO_HANDLER_Init();

            /// <summary>
            /// Initialize.
            /// </summary>
            /// <returns></returns>
            public static bool Init()
            {
                return BASS_ASIO_HANDLER_Init();
            }

            [DllImport(DllName)]
            static extern bool BASS_ASIO_HANDLER_Free();

            /// <summary>
            /// Free.
            /// </summary>
            /// <returns></returns>
            public static bool Free()
            {
                return BASS_ASIO_HANDLER_Free();
            }

            [DllImport(DllName)]
            static extern bool BASS_ASIO_HANDLER_StreamGet(out int Handle);

            public static bool StreamGet(out int Handle)
            {
                return BASS_ASIO_HANDLER_StreamGet(out Handle);
            }

            [DllImport(DllName)]
            static extern bool BASS_ASIO_HANDLER_StreamSet(int Handle);

            public static bool StreamSet(int Handle)
            {
                return BASS_ASIO_HANDLER_StreamSet(Handle);
            }

            [DllImport(DllName)]
            static extern bool BASS_ASIO_HANDLER_ChannelEnable(bool Input, int Channel, IntPtr User = default(IntPtr));

            public static bool ChannelEnable(bool Input, int Channel, IntPtr User = default(IntPtr))
            {
                return BASS_ASIO_HANDLER_ChannelEnable(Input, Channel, User);
            }
        }
    }
}
