using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Asio;
using ManagedBass.Mix;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassAsioStreamOutput : BassStreamOutput
    {
        private BassAsioStreamOutput()
        {
            this.MixerChannelHandles = new HashSet<int>();
        }

        public BassAsioStreamOutput(BassAsioStreamOutputBehaviour behaviour, BassOutputStream stream)
            : this()
        {
            this.Behaviour = behaviour;
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
                var rate = default(int);
                var channels = default(int);
                var flags = default(BassFlags);
                if (!this.GetFormat(out rate, out channels, out flags))
                {
                    rate = 0;
                    channels = 0;
                    flags = BassFlags.Default;
                }
                return string.Format(
                    "{0} ({1}/{2}/{3})",
                    this.Name,
                    BassUtils.DepthDescription(flags),
                    MetaDataInfo.SampleRateDescription(rate),
                    MetaDataInfo.ChannelDescription(channels)
                );
            }
        }

        public HashSet<int> MixerChannelHandles { get; protected set; }

        public BassAsioStreamOutputBehaviour Behaviour { get; private set; }

        public override int ChannelHandle { get; protected set; }

        protected override IEnumerable<int> GetMixerChannelHandles()
        {
            return this.MixerChannelHandles;
        }

        public override bool CheckFormat(int rate, int channels)
        {
            return BassAsioDevice.Info.SupportedRates.Contains(rate) && channels <= BassAsioDevice.Info.Outputs;
        }

        public override void Connect(IBassStreamComponent previous)
        {
            var rate = default(int);
            var channels = default(int);
            var flags = default(BassFlags);
            this.ConfigureASIO(previous, out rate, out channels, out flags);
            if (this.ShouldCreateMixer(previous, rate, channels, flags))
            {
                Logger.Write(this, LogLevel.Debug, "Creating BASS MIX stream with rate {0} and {1} channels.", rate, channels);
                this.ChannelHandle = BassMix.CreateMixerStream(rate, channels, flags);
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
                this.ChannelHandle = previous.ChannelHandle;
            }
            this.UpdateVolume();
        }

        protected virtual void ConfigureASIO(IBassStreamComponent previous, out int rate, out int channels, out BassFlags flags)
        {
            previous.GetFormat(out rate, out channels, out flags);
            if (flags.HasFlag(BassFlags.DSDRaw))
            {
                //Nothing to do.
            }
            else
            {
                if (this.Behaviour.Output.EnforceRate)
                {
                    var targetRate = this.Behaviour.Output.Rate;
                    if (!BassAsioDevice.Info.SupportedRates.Contains(targetRate))
                    {
                        var nearestRate = BassAsioDevice.Info.GetNearestRate(targetRate);
                        Logger.Write(this, LogLevel.Warn, "Enforced rate {0} isn't supposed by the device, falling back to {1}.", targetRate, nearestRate);
                        rate = nearestRate;
                    }
                    else
                    {
                        rate = targetRate;
                    }
                }
                else
                {
                    if (!BassAsioDevice.Info.SupportedRates.Contains(rate))
                    {
                        var nearestRate = BassAsioDevice.Info.GetNearestRate(rate);
                        Logger.Write(this, LogLevel.Debug, "Stream rate {0} isn't supposed by the device, falling back to {1}.", rate, nearestRate);
                        rate = nearestRate;
                    }
                }
            }
            BassAsioDevice.Init(
                this.Behaviour.AsioDevice,
                rate,
                channels,
                flags
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

        protected virtual bool ShouldCreateMixer(IBassStreamComponent previous, int rate, int channels, BassFlags flags)
        {
            if (flags.HasFlag(BassFlags.DSDRaw))
            {
                //Can't create mixer for DSD.
                return false;
            }
            if (this.Behaviour.Mixer)
            {
                //Mixer is forced on, probably so visualizations work.
                return true;
            }
            var _rate = default(int);
            var _channels = default(int);
            var _flags = default(BassFlags);
            previous.GetFormat(out _rate, out _channels, out _flags);
            if (rate != _rate || channels != _channels)
            {
                //Stream rate or channel count differs from device.
                return true;
            }
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
                this.ErrorEmitter.Send(e);
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
                this.ErrorEmitter.Send(e);
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
                this.ErrorEmitter.Send(e);
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
                this.ErrorEmitter.Send(e);
                throw;
            }
        }

        protected override float GetVolume()
        {
            if (!BassAsioDevice.CanControlVolume)
            {
                return 1;
            }
            return BassAsioDevice.Volume;
        }

        protected override void SetVolume(float volume)
        {
            if (!BassAsioDevice.CanControlVolume)
            {
                return;
            }
            BassAsioDevice.Volume = volume;
        }

        protected override void OnDisposing()
        {
            if (this.ChannelHandle != 0)
            {
                Logger.Write(this, LogLevel.Debug, "Freeing BASS stream: {0}", this.ChannelHandle);
                BassUtils.OK(Bass.StreamFree(this.ChannelHandle)); //Not checking result code as it contains an error if the application is shutting down.
            }
            this.Stop();
            BassAsioDevice.Free();
            base.OnDisposing();
        }
    }
}
