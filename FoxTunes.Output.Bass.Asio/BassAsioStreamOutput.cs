using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Asio;
using ManagedBass.Mix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FoxTunes
{
    public class BassAsioStreamOutput : BassStreamOutput
    {
        private BassAsioStreamOutput()
        {
            this.Flags = BassFlags.Default;
            this.MixerChannelHandles = new HashSet<int>();
        }

        public BassAsioStreamOutput(BassAsioStreamOutputBehaviour behaviour, BassOutputStream stream)
            : this()
        {
            this.Behaviour = behaviour;
            this.Rate = behaviour.Output.Rate;
            this.Channels = BassAsioDevice.Info.Outputs;
            this.Flags = BassFlags.Decode;
            if (this.Behaviour.Output.Float)
            {
                this.Flags |= BassFlags.Float;
            }
        }

        public override string Name
        {
            get
            {
                return "ASIO";
            }
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    "{0} ({1}/{2}/{3})",
                    this.Name,
                    BassUtils.DepthDescription(this.Flags),
                    MetaDataInfo.SampleRateDescription(this.Rate),
                    MetaDataInfo.ChannelDescription(this.Channels)
                );
            }
        }

        public BassAsioStreamOutputBehaviour Behaviour { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public HashSet<int> MixerChannelHandles { get; protected set; }

        public override bool CanControlVolume
        {
            get
            {
                return BassAsioDevice.CanControlVolume;
            }
        }

        public override float Volume
        {
            get
            {
                return BassAsioDevice.Volume;
            }
            set
            {
                BassAsioDevice.Volume = value;
            }
        }

        public override bool CheckFormat(int rate, int channels)
        {
            return BassAsioDevice.Info.SupportedRates.Contains(rate) && channels <= BassAsioDevice.Info.Outputs;
        }

        public override void Connect(IBassStreamComponent previous)
        {
            this.ConfigureASIO(previous);
            if (this.ShouldCreateMixer(previous))
            {
                Logger.Write(this, LogLevel.Debug, "Creating BASS MIX stream with rate {0} and {1} channels.", this.Rate, this.Channels);
                this.ChannelHandle = BassMix.CreateMixerStream(this.Rate, this.Channels, this.Flags);
                if (this.ChannelHandle == 0)
                {
                    BassUtils.Throw();
                }
                Logger.Write(this, LogLevel.Debug, "Adding stream to the mixer: {0}", previous.ChannelHandle);
                BassUtils.OK(BassMix.MixerAddChannel(this.ChannelHandle, previous.ChannelHandle, BassFlags.Default | BassFlags.MixerBuffer));
                BassUtils.OK(BassAsioHandler.StreamSet(this.ChannelHandle));
                this.MixerChannelHandles.Add(previous.ChannelHandle);
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "The stream properties match the device, playing directly.");
                BassUtils.OK(BassAsioHandler.StreamSet(previous.ChannelHandle));
            }
        }

        protected virtual void ConfigureASIO(IBassStreamComponent previous)
        {
            if (previous.Flags.HasFlag(BassFlags.DSDRaw))
            {
                this.Rate = previous.Rate;
                this.Flags |= BassFlags.DSDRaw;
            }
            else
            {
                if (this.Behaviour.Output.EnforceRate)
                {
                    if (!BassAsioDevice.Info.SupportedRates.Contains(this.Rate))
                    {
                        var nearestRate = BassAsioDevice.Info.GetNearestRate(this.Rate);
                        Logger.Write(this, LogLevel.Warn, "Enforced rate {0} isn't supposed by the device, falling back to {1}.", this.Rate, nearestRate);
                        this.Rate = nearestRate;
                    }
                    else
                    {
                        //Enfoced rate is supported by the device, nothing to do.
                    }
                }
                else
                {
                    if (!BassAsioDevice.Info.SupportedRates.Contains(previous.Rate))
                    {
                        var nearestRate = BassAsioDevice.Info.GetNearestRate(previous.Rate);
                        Logger.Write(this, LogLevel.Debug, "Stream rate {0} isn't supposed by the device, falling back to {1}.", this.Rate, nearestRate);
                        this.Rate = nearestRate;
                    }
                    else
                    {
                        //Stream rate is supported by the device, nothing to do.
                        this.Rate = previous.Rate;
                    }
                }
            }
            BassAsioDevice.Init(
                this.Behaviour.AsioDevice,
                this.Rate,
                this.Channels,
                this.Flags
            );
        }

        protected virtual bool StartASIO()
        {
            if (BassAsio.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "ASIO has already been started.");
                return false;
            }
            Logger.Write(this, LogLevel.Debug, "Starting ASIO.");
            BassUtils.OK(BassAsio.Start(BassAsio.Info.PreferredBufferLength));
            return true;
        }

        protected virtual bool StopASIO()
        {
            if (!BassAsio.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "ASIO has not been started.");
                return false;
            }
            Logger.Write(this, LogLevel.Debug, "Stopping ASIO.");
            BassAsioUtils.OK(BassAsio.Stop());
            return true;
        }

        protected virtual bool ShouldCreateMixer(IBassStreamComponent previous)
        {
            if (previous.Flags.HasFlag(BassFlags.DSDRaw))
            {
                //Can't create mixer for DSD.
                return false;
            }
            if (this.Behaviour.Mixer)
            {
                //Mixer is forced on, probably so visualizations work.
                return true;
            }
            else if (previous.Rate != this.Rate || previous.Channels != this.Channels)
            {
                //Stream rate or channel count differs from device.
                return true;
            }
            //Looks like no mixer is required.
            return false;
        }

        public override bool IsPlaying
        {
            get
            {
                return BassAsio.IsStarted;
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsPaused
        {
            get
            {
                return BassAsio.ChannelIsActive(false, BassAsioDevice.PRIMARY_CHANNEL) == AsioChannelActive.Paused;
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsStopped
        {
            get
            {
                return !BassAsio.IsStarted;
            }
            protected set
            {
                throw new NotImplementedException();
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
            if (this.IsPlaying)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Starting ASIO.");
            try
            {
                BassAsioUtils.OK(this.StartASIO());
            }
            catch (Exception e)
            {
                this.OnError(e);
                throw;
            }
        }

        public override void Pause()
        {
            if (this.IsPaused)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Pausing ASIO.");
            try
            {
                BassAsioUtils.OK(BassAsio.ChannelPause(false, BassAsioDevice.PRIMARY_CHANNEL));
            }
            catch (Exception e)
            {
                this.OnError(e);
                throw;
            }
        }

        public override void Resume()
        {
            if (!this.IsPaused)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Resuming ASIO.");
            try
            {
                BassAsioUtils.OK(BassAsio.ChannelReset(false, BassAsioDevice.PRIMARY_CHANNEL, AsioChannelResetFlags.Pause));
            }
            catch (Exception e)
            {
                this.OnError(e);
                throw;
            }
        }

        public override void Stop()
        {
            if (this.IsStopped)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Stopping ASIO.");
            try
            {
                BassAsioUtils.OK(this.StopASIO());
            }
            catch (Exception e)
            {
                this.OnError(e);
                throw;
            }
        }

        public override int GetData(float[] buffer)
        {
            var length = default(uint);
            switch (buffer.Length)
            {
                case 128:
                    length = FFT256;
                    break;
                case 256:
                    length = FFT512;
                    break;
                case 512:
                    length = FFT1024;
                    break;
                default:
                    throw new NotImplementedException();
            }
            foreach (var channelHandle in this.MixerChannelHandles)
            {
                return BassMix.ChannelGetData(channelHandle, buffer, unchecked((int)length));
            }
            return 0;
        }

        protected override void OnDisposing()
        {
            this.Stop();
            BassAsioDevice.Free();
        }
    }
}
