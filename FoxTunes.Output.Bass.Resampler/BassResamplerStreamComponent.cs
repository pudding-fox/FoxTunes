using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Sox;
using System;

namespace FoxTunes
{
    public class BassResamplerStreamComponent : BassStreamComponent, IBassStreamControllable
    {
        private readonly Metric BufferLengthMetric = new Metric(3);

        public static bool KEEP_ALIVE = true;

        public static int BUFFER_LENGTH = 3;

        public BassResamplerStreamComponent(BassResamplerStreamComponentBehaviour behaviour, BassOutputStream stream)
        {
            if (BassUtils.GetChannelDsdRaw(stream.ChannelHandle))
            {
                throw new InvalidOperationException("Cannot resample DSD streams.");
            }
            this.Behaviour = behaviour;
            this.Rate = behaviour.Output.Rate;
            this.Channels = stream.Channels;
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
                return "Resampler";
            }
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    "{0} ({1}/{2} -> {1}/{3})",
                    this.Name,
                    BassUtils.DepthDescription(this.Flags),
                    BassUtils.RateDescription(this.InputRate),
                    BassUtils.RateDescription(this.Rate)
                );
            }
        }

        public BassResamplerStreamComponentBehaviour Behaviour { get; private set; }

        public int InputRate { get; protected set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public override long BufferLength
        {
            get
            {
                var length = default(int);
                BassUtils.OK(BassSox.StreamBufferLength(this.ChannelHandle, out length));
                return this.BufferLengthMetric.Average(length);
            }
        }

        public override void Connect(IBassStreamComponent previous)
        {
            Logger.Write(this, LogLevel.Debug, "Creating BASS SOX stream with rate {0} => {1} and {2} channels.", previous.Rate, this.Rate, this.Channels);
            this.InputRate = previous.Rate;
            this.ChannelHandle = BassSox.StreamCreate(this.Rate, this.Flags, previous.ChannelHandle);
            if (this.ChannelHandle == 0)
            {
                BassUtils.Throw();
            }
            BassUtils.OK(BassSox.ChannelSetAttribute(this.ChannelHandle, SoxChannelAttribute.KeepAlive, KEEP_ALIVE));
            BassUtils.OK(BassSox.ChannelSetAttribute(this.ChannelHandle, SoxChannelAttribute.BufferLength, BUFFER_LENGTH));
        }

        public override void ClearBuffer()
        {
            Logger.Write(this, LogLevel.Debug, "Clearing BASS SOX buffer: {0}", this.ChannelHandle);
            BassUtils.OK(BassSox.StreamBufferClear(this.ChannelHandle));
            this.BufferLengthMetric.Reset();
        }

        public bool IsBackground
        {
            get
            {
                var background = default(int);
                BassUtils.OK(BassSox.ChannelGetAttribute(this.ChannelHandle, SoxChannelAttribute.Background, out background));
                return Convert.ToBoolean(background);
            }
            set
            {
                if (this.IsBackground == value)
                {
                    return;
                }
                Logger.Write(this, LogLevel.Debug, "Setting BASS SOX attribute \"{0}\" = \"{1}\"", Enum.GetName(typeof(SoxChannelAttribute), SoxChannelAttribute.Background), value);
                BassUtils.OK(BassSox.ChannelSetAttribute(this.ChannelHandle, SoxChannelAttribute.Background, value));
            }
        }

        public void Play()
        {
            this.IsBackground = true;
        }

        public void Pause()
        {
            this.IsBackground = false;
        }

        public void Resume()
        {
            this.IsBackground = true;
        }

        public void Stop()
        {
            this.IsBackground = false;
        }

        protected override void OnDisposing()
        {
            if (this.ChannelHandle != 0)
            {
                Logger.Write(this, LogLevel.Debug, "Freeing BASS SOX stream: {0}", this.ChannelHandle);
                BassUtils.OK(BassSox.StreamFree(this.ChannelHandle));
            }
        }
    }
}
