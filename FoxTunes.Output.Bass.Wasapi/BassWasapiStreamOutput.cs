using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Wasapi;
using System;
using System.Linq;
using System.Threading;

namespace FoxTunes
{
    public class BassWasapiStreamOutput : BassStreamOutput
    {
        const int CONNECT_ATTEMPTS = 5;

        const int CONNECT_ATTEMPT_INTERVAL = 400;

        const int START_ATTEMPTS = 5;

        const int START_ATTEMPT_INTERVAL = 400;

        private BassWasapiStreamOutput()
        {
            this.Flags = BassFlags.Default;
        }

        public BassWasapiStreamOutput(BassWasapiStreamOutputBehaviour behaviour, BassOutputStream stream)
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
                else if (!behaviour.Output.EnforceRate && BassWasapiDevice.Info.SupportedRates.Contains(stream.Rate))
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

        public BassWasapiStreamOutputBehaviour Behaviour { get; private set; }

        public int Device
        {
            get
            {
                return BassWasapiDevice.Device;
            }
        }

        public override int Rate { get; protected set; }

        public override int Depth { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public override bool CheckFormat(int rate, int channels)
        {
            return BassWasapiDevice.Info.SupportedRates.Contains(rate) && channels <= BassWasapiDevice.Info.Outputs;
        }

        public override void Connect(IBassStreamComponent previous)
        {
            if (previous.Channels > BassWasapiDevice.Info.Outputs)
            {
                //TODO: We should down mix.
                Logger.Write(this, LogLevel.Error, "Cannot play stream with more channels than device outputs.");
                throw new NotImplementedException(string.Format("The stream contains {0} channels which is greater than {1} output channels provided by the device.", this.Channels, BassWasapiDevice.Info.Outputs));
            }
            if (!this.CheckFormat(this.Rate, previous.Channels))
            {
                Logger.Write(this, LogLevel.Error, "Cannot play stream with unsupported rate.");
                throw new NotImplementedException(string.Format("The stream has a rate of {0} which is not supported by the device.", this.Rate));
            }
            if (this.Rate != previous.Rate)
            {
                Logger.Write(this, LogLevel.Error, "Cannot play stream with different rate to device.");
                throw new NotImplementedException(string.Format("Cannot play stream with rate {0} not equal to device rate {1}, you must configure resampling.", previous.Rate, this.Rate));
            }
            if (!BassUtils.GetChannelFloat(previous.ChannelHandle))
            {
                Logger.Write(this, LogLevel.Error, "Cannot play non float stream.");
                throw new NotImplementedException("Cannot play non 32 bit float stream, you must configure depth.");
            }
            var exception = default(Exception);
            for (var a = 1; a <= CONNECT_ATTEMPTS; a++)
            {
                Logger.Write(this, LogLevel.Debug, "Configuring WASAPI, attempt: {0}", a);
                try
                {
                    if (BassUtils.OK(this.ConfigureWASAPI(previous)))
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                    Logger.Write(this, LogLevel.Warn, "Failed to configure WASAPI: {0}", e.Message);
                    if (BassWasapiDevice.IsInitialized)
                    {
                        Logger.Write(this, LogLevel.Warn, "Re-initializing WASAPI, have you just switched from DSD to PCM?");
                        BassWasapiDevice.Free();
                        BassWasapiDevice.Init();
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

        protected virtual bool StartWASAPI()
        {
            for (var a = 1; a <= START_ATTEMPTS; a++)
            {
                Logger.Write(this, LogLevel.Debug, "Starting WASAPI, attempt: {0}", a);
                try
                {
                    var success = BassWasapi.Start();
                    if (success)
                    {
                        Logger.Write(this, LogLevel.Debug, "Successfully started WASAPI.");
                        return true;
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Warn, "Failed to start WASAPI: {0}", Enum.GetName(typeof(Errors), Bass.LastError));
                    }
                }
                catch (Exception e)
                {
                    //Nothing can be done.
                    Logger.Write(this, LogLevel.Warn, "Failed to start WASAPI: {0}", e.Message);
                }
                Thread.Sleep(START_ATTEMPT_INTERVAL);
            }
            Logger.Write(this, LogLevel.Warn, "Failed to start WASAPI after {0} attempts.", START_ATTEMPTS);
            return false;
        }

        protected virtual bool StopWASAPI(bool reset = true)
        {
            if (!BassWasapi.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "WASAPI has not been started.");
                return false;
            }
            Logger.Write(this, LogLevel.Debug, "Stopping WASAPI.");
            BassUtils.OK(BassWasapi.Stop(reset));
            return true;
        }

        protected virtual bool ConfigureWASAPI(IBassStreamComponent previous)
        {
            Logger.Write(this, LogLevel.Debug, "Configuring WASAPI.");

            if (!this.CheckFormat(previous.Rate, previous.Channels))
            {
                throw new NotImplementedException();
            }

            BassWasapiDevice.Init(previous.Rate, previous.Channels);

            BassUtils.OK(BassWasapiHandler.StreamSet(previous.ChannelHandle));

            return true;
        }

        public override bool IsPlaying
        {
            get
            {
                return BassWasapi.IsStarted;
            }
        }

        public override bool IsPaused
        {
            get
            {
                return false;
            }
        }

        public override bool IsStopped
        {
            get
            {
                return !BassWasapi.IsStarted;
            }
        }

        public override int Latency
        {
            get
            {
                return 0;
            }
        }

        public override void Play()
        {
            if (this.IsPlaying)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Starting WASAPI.");
            try
            {
                BassUtils.OK(this.StartWASAPI());
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public override void Pause()
        {
            throw new NotImplementedException();
        }

        public override void Resume()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            if (this.IsStopped)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Stopping WASAPI.");
            try
            {
                BassUtils.OK(this.StopWASAPI());
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        protected override void OnDisposing()
        {
            if (BassWasapi.IsStarted)
            {
                BassUtils.OK(this.StopWASAPI());
            }
        }
    }
}
