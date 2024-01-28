using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Asio;
using System;
using System.Linq;
using System.Threading;

namespace FoxTunes
{
    public class BassAsioStreamOutput : BassStreamOutput
    {
        const int CONNECT_ATTEMPTS = 5;

        const int CONNECT_ATTEMPT_INTERVAL = 400;

        const int START_ATTEMPTS = 5;

        const int START_ATTEMPT_INTERVAL = 400;

        private BassAsioStreamOutput()
        {
            this.Flags = BassFlags.Default;
        }

        public BassAsioStreamOutput(BassAsioStreamOutputBehaviour behaviour, BassOutputStream stream)
            : this()
        {
            this.Behaviour = behaviour;
            if (BassUtils.GetChannelDsdRaw(stream.ChannelHandle))
            {
                this.Rate = BassUtils.GetChannelDsdRate(stream.ChannelHandle);
                this.Flags |= BassFlags.DSDRaw;
            }
            else
            {
                if (behaviour.Output.Rate == stream.Rate)
                {
                    this.Rate = stream.Rate;
                }
                else if (!behaviour.Output.EnforceRate && BassAsioDevice.Info.SupportedRates.Contains(stream.Rate))
                {
                    this.Rate = stream.Rate;
                }
                else
                {
                    Logger.Write(this, LogLevel.Debug, "The requested output rate is either enforced or the device does not support the stream's rate: {0} => {1}", stream.Rate, behaviour.Output.Rate);
                    this.Rate = behaviour.Output.Rate;
                }
                if (behaviour.Output.Float)
                {
                    this.Flags |= BassFlags.Float;
                }
            }
            this.Depth = stream.Depth;
            this.Channels = stream.Channels;
        }

        public BassAsioStreamOutputBehaviour Behaviour { get; private set; }

        public int Device
        {
            get
            {
                return BassAsioDevice.Device;
            }
        }

        public override int Rate { get; protected set; }

        public override int Depth { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public override bool CheckFormat(int rate, int channels)
        {
            return BassAsio.CheckRate(rate) && channels <= BassAsio.Info.Outputs;
        }

        public override void Connect(IBassStreamComponent previous)
        {
            if (previous.Channels > BassAsio.Info.Outputs)
            {
                //TODO: We should down mix.
                Logger.Write(this, LogLevel.Error, "Cannot play stream with more channels than device outputs.");
                throw new NotImplementedException(string.Format("The stream contains {0} channels which is greater than {1} output channels provided by the device.", this.Channels, BassAsio.Info.Outputs));
            }
            if (!BassAsio.CheckRate(this.Rate))
            {
                Logger.Write(this, LogLevel.Error, "Cannot play stream with unsupported rate.");
                throw new NotImplementedException(string.Format("The stream has a rate of {0} which is not supported by the device.", this.Rate));
            }
            var exception = default(Exception);
            for (var a = 1; a <= CONNECT_ATTEMPTS; a++)
            {
                Logger.Write(this, LogLevel.Debug, "Configuring ASIO, attempt: {0}", a);
                try
                {
                    if (BassAsioUtils.OK(this.ConfigureASIO(previous)))
                    {
                        var success = default(bool);
                        if (previous.Flags.HasFlag(BassFlags.DSDRaw) || BassUtils.GetChannelDsdRaw(previous.ChannelHandle))
                        {
                            success = BassAsioUtils.OK(this.ConfigureASIO_DSD(previous));
                        }
                        else
                        {
                            success = BassAsioUtils.OK(this.ConfigureASIO_PCM(previous));
                        }
                        if (success)
                        {
                            Logger.Write(this, LogLevel.Debug, "Configured ASIO.");
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                    Logger.Write(this, LogLevel.Warn, "Failed to configure ASIO: {0}", e.Message);
                    if (BassAsioDevice.IsInitialized)
                    {
                        Logger.Write(this, LogLevel.Warn, "Re-initializing ASIO, have you just switched from DSD to PCM?");
                        BassAsioDevice.Free();
                        BassAsioDevice.Init();
                    }
                }
                Thread.Sleep(CONNECT_ATTEMPT_INTERVAL);
            }
            if (exception != null)
            {
                throw exception;
            }
            throw new NotImplementedException();
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
            BassAsioUtils.OK(BassAsio.Stop());
            return true;
        }

        protected virtual bool ConfigureASIO(IBassStreamComponent previous)
        {
            Logger.Write(this, LogLevel.Debug, "Configuring ASIO.");
            BassAsioUtils.OK(BassAsioHandler.StreamSet(previous.ChannelHandle));
            BassAsioUtils.OK(BassAsioHandler.ChannelEnable(false, BassAsioDevice.PRIMARY_CHANNEL));
            if (previous.Channels == 1)
            {
                Logger.Write(this, LogLevel.Debug, "Mirroring channel: {0} => {1}", BassAsioDevice.PRIMARY_CHANNEL, BassAsioDevice.SECONDARY_CHANNEL);
                BassAsioUtils.OK(BassAsio.ChannelEnableMirror(BassAsioDevice.SECONDARY_CHANNEL, false, BassAsioDevice.PRIMARY_CHANNEL));
            }
            else
            {
                for (var channel = 1; channel < previous.Channels; channel++)
                {
                    Logger.Write(this, LogLevel.Debug, "Joining channel: {0} => {1}", channel, BassAsioDevice.PRIMARY_CHANNEL);
                    BassAsioUtils.OK(BassAsio.ChannelJoin(false, channel, BassAsioDevice.PRIMARY_CHANNEL));
                }
            }
            return true;
        }

        protected virtual bool ConfigureASIO_PCM(IBassStreamComponent previous)
        {
            Logger.Write(this, LogLevel.Debug, "Configuring PCM.");
            BassAsioUtils.OK(BassAsio.SetDSD(false));
            if (!this.CheckFormat(this.Rate, this.Channels))
            {
                Logger.Write(this, LogLevel.Warn, "PCM format {0}:{1} is unsupported.", this.Rate, this.Channels);
                return false;
            }
            else
            {
                BassAsio.Rate = this.Rate;
            }
            var format = default(AsioSampleFormat);
            if (previous.Flags.HasFlag(BassFlags.Float))
            {
                format = AsioSampleFormat.Float;
            }
            else
            {
                format = AsioSampleFormat.Bit16;
            }
            Logger.Write(this, LogLevel.Debug, "PCM: Rate = {0}, Format = {1}", BassAsio.Rate, Enum.GetName(typeof(AsioSampleFormat), format));
            BassAsioUtils.OK(BassAsio.ChannelSetRate(false, BassAsioDevice.PRIMARY_CHANNEL, previous.Rate));
            BassAsioUtils.OK(BassAsio.ChannelSetFormat(false, BassAsioDevice.PRIMARY_CHANNEL, format));
            return true;
        }

        protected virtual bool ConfigureASIO_DSD(IBassStreamComponent previous)
        {
            try
            {
                Logger.Write(this, LogLevel.Debug, "Configuring DSD RAW.");
                try
                {
                    BassAsioUtils.OK(BassAsio.SetDSD(true));
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
                if (!this.CheckFormat(this.Rate, previous.Channels))
                {
                    Logger.Write(this, LogLevel.Warn, "DSD format {0}:{1} is unsupported.", previous.Rate, previous.Channels);
                    return false;
                }
                else
                {
                    BassAsio.Rate = this.Rate;
                }
                //It looks like BASS DSD always outputs 8 bit/MSB data so we don't need to determine the format.
                //var format = default(AsioSampleFormat);
                //switch (this.Depth)
                //{
                //    case BassAttribute.DSDFormat_LSB:
                //        format = AsioSampleFormat.DSD_LSB;
                //        break;
                //    case BassAttribute.DSDFormat_None:
                //    case BassAttribute.DSDFormat_MSB:
                //        format = AsioSampleFormat.DSD_MSB;
                //        break;
                //    default:
                //        throw new NotImplementedException();
                //}
                Logger.Write(this, LogLevel.Debug, "DSD: Rate = {0}, Depth = {1}, Format = {2}", BassAsio.Rate, this.Depth, Enum.GetName(typeof(AsioSampleFormat), AsioSampleFormat.DSD_MSB));
                BassAsioUtils.OK(BassAsio.ChannelSetFormat(false, BassAsioDevice.PRIMARY_CHANNEL, AsioSampleFormat.DSD_MSB));
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
                for (var channel = 0; channel < this.Channels; channel++)
                {
                    BassAsioUtils.OK(BassAsio.ChannelReset(false, channel, flags));
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
                return BassAsio.ChannelIsActive(false, BassAsioDevice.PRIMARY_CHANNEL) == AsioChannelActive.Paused;
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
            }
        }

        public override void Resume()
        {
            if (this.IsPlaying)
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
            }
        }

        protected override void OnDisposing()
        {
            if (BassAsio.IsStarted)
            {
                BassAsioUtils.OK(this.StopASIO());
                BassAsioUtils.OK(this.ResetASIO());
            }
        }
    }
}
