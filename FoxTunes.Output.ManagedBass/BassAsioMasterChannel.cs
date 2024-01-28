using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Asio;
using System;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public class BassAsioMasterChannel : BassMasterChannel
    {
        const int PRIMARY_CHANNEL = 0;

        public BassAsioMasterChannel(BassOutput output) : base(output)
        {

        }

        public override BassFlags Flags
        {
            get
            {
                return base.Flags | BassFlags.Decode;
            }
        }

        public AsioSampleFormat SampleFormat
        {
            get
            {
                if (this.Output.Float)
                {
                    return AsioSampleFormat.Float;
                }
                return AsioSampleFormat.Bit16;
            }
        }

        public AsioSampleFormat DSDSampleFormat
        {
            get
            {
                return AsioSampleFormat.DSD_MSB;
            }
        }

        protected override void StartStream(BassMasterChannelConfig config)
        {
            base.StartStream(config);
            try
            {
                this.Setup();
                if (this.Config.DsdDirect && this.Config.DsdRate > 0)
                {
                    this.SetupDsd();
                }
                else
                {
                    this.SetupPcm();
                }
                Logger.Write(this, LogLevel.Debug, "Enabled ASIO on master stream: {0}", this.ChannelHandle);
            }
            catch
            {
                this.IsStarted = false;
                throw;
            }
        }

        protected virtual void Setup()
        {
            BassUtils.OK(BassAsio.Init(this.Output.AsioDevice, AsioInitFlags.Thread));
            BassUtils.OK(BASS_ASIO_ChannelEnableGaplessMaster(false, PRIMARY_CHANNEL, IntPtr.Zero));
            for (var a = 1; a < Math.Min(BassAsio.Info.Outputs, this.Config.Channels); a++)
            {
                BassUtils.OK(BassAsio.ChannelJoin(false, a, PRIMARY_CHANNEL));
            }
        }

        protected virtual void SetupDsd()
        {
            BassUtils.OK(BassAsio.SetDSD(true));
            BassUtils.OK(BassAsio.ChannelSetFormat(false, PRIMARY_CHANNEL, this.DSDSampleFormat));
            if (!BassAsio.CheckRate(this.Config.EffectiveRate))
            {
                Logger.Write(this, LogLevel.Warn, "ASIO driver reports that DSD rate {0} is not supported and we can't currently resample DSD. Playback is likely to fail.", this.Config.EffectiveRate);
            }
            BassAsio.Rate = this.Config.EffectiveRate;
            Logger.Write(this, LogLevel.Debug, "Configured ASIO for DSD: {0}/{1}", BassAsio.Rate, Enum.GetName(typeof(AsioSampleFormat), this.DSDSampleFormat));
        }

        protected virtual void SetupPcm()
        {
            BassUtils.OK(BassAsio.SetDSD(false));
            BassUtils.OK(BassAsio.ChannelSetFormat(false, PRIMARY_CHANNEL, this.SampleFormat));
            BassUtils.OK(BassAsio.ChannelSetRate(false, PRIMARY_CHANNEL, this.Config.EffectiveRate));
            var desiredRate = default(int);
            if (this.Output.EnforceRate)
            {
                desiredRate = this.Output.Rate;
            }
            else
            {
                desiredRate = Math.Min(this.Config.EffectiveRate, this.Output.Rate);
                if (desiredRate != this.Output.Rate && !BassAsio.CheckRate(desiredRate))
                {
                    Logger.Write(this, LogLevel.Warn, "ASIO driver reports that PCM rate {0} is not supported, falling back to", desiredRate, this.Output.Rate);
                    desiredRate = this.Output.Rate;
                }
            }
            if (this.Config.Channels > BassAsio.Info.Outputs)
            {
                Logger.Write(this, LogLevel.Warn, "Stream has {0} channels which is greater than {1} channels supported by the output.", this.Config.Channels, BassAsio.Info.Outputs);
            }
            if (this.Config.EffectiveRate != desiredRate)
            {
                Logger.Write(this, LogLevel.Warn, "Stream has rate {0} which is not equal to {1} configured for the device, resampling will occure.", this.Config.EffectiveRate, desiredRate);
            }
            BassAsio.Rate = desiredRate;
            Logger.Write(this, LogLevel.Debug, "Configured ASIO for PCM: {0}/{1}", BassAsio.Rate, Enum.GetName(typeof(AsioSampleFormat), this.SampleFormat));
        }

        protected override void StopStream()
        {
            if (!this.IsStarted)
            {
                return;
            }
            try
            {
                if (BassAsio.IsStarted)
                {
                    BassUtils.OK(BassAsio.Stop());
                }
                BassUtils.OK(BassAsio.ChannelReset(false, PRIMARY_CHANNEL, AsioChannelResetFlags.Enable));
                BassUtils.OK(BassAsio.ChannelReset(false, PRIMARY_CHANNEL, AsioChannelResetFlags.Format));
                BassUtils.OK(BassAsio.ChannelReset(false, PRIMARY_CHANNEL, AsioChannelResetFlags.Rate));
                for (var a = 1; a < Math.Min(BassAsio.Info.Outputs, this.Config.Channels); a++)
                {
                    BassUtils.OK(BassAsio.ChannelReset(false, a, AsioChannelResetFlags.Join));
                }
                BassUtils.OK(BassAsio.Free());
                Logger.Write(this, LogLevel.Debug, "Disabled ASIO on master stream: {0}", this.ChannelHandle);
            }
            finally
            {
                base.StopStream();
            }
        }

        public override void SetPrimaryChannel(int channelHandle)
        {
            base.SetPrimaryChannel(channelHandle);
            if (channelHandle != 0)
            {
                if (!BassAsio.IsStarted)
                {
                    BassUtils.OK(BassAsio.Start(BassAsio.Info.PreferredBufferLength));
                }
            }
            else
            {
                if (BassAsio.IsStarted)
                {
                    BassUtils.OK(BassAsio.Stop());
                }
            }
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

        public override void Play()
        {
            if (BassAsio.IsStarted)
            {
                return;
            }
            BassUtils.OK(BassAsio.Start(BassAsio.Info.PreferredBufferLength));
        }

        public override void Pause()
        {
            BassUtils.OK(BassAsio.ChannelPause(false, PRIMARY_CHANNEL));
        }

        public override void Resume()
        {
            BassUtils.OK(BassAsio.ChannelReset(false, PRIMARY_CHANNEL, AsioChannelResetFlags.Pause));
        }

        public override void Stop()
        {
            if (!BassAsio.IsStarted)
            {
                return;
            }
            BassUtils.OK(BassAsio.Stop());
        }

        [DllImport("bass_foxtunes.dll", EntryPoint = "BASS_ASIO_ChannelEnableGaplessMaster")]
        static extern int BASS_ASIO_ChannelEnableGaplessMaster(bool Input, int Channel, IntPtr User);
    }
}
