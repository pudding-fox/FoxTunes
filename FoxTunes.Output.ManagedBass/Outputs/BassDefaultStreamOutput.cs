using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Mix;
using System;

namespace FoxTunes
{
    public class BassDefaultStreamOutput : BassStreamOutput
    {
        public static void Init(IBassOutput output)
        {
            BassUtils.OK(Bass.Configure(Configuration.UpdateThreads, 1));
            BassUtils.OK(Bass.Init(output.DirectSoundDevice, output.Rate));
            Logger.Write(typeof(BassDefaultStreamOutput), LogLevel.Debug, "BASS Initialized.");
        }

        public static void Free()
        {
            //Nothing to do.
        }

        public BassDefaultStreamOutput(int rate, int channels, BassFlags flags)
        {
            this.Rate = rate;
            this.Channels = channels;
            this.Flags = flags & ~BassFlags.Decode;
        }

        public override BassStreamOutputCapability Capabilities
        {
            get
            {
                return BassStreamOutputCapability.None;
            }
        }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public override bool CheckFormat(int rate, int channels)
        {
            return true;
        }

        public override void Connect(IBassStreamComponent previous)
        {
            Logger.Write(this, LogLevel.Debug, "Creating BASS MIX stream with rate {0} and {1} channels.", this.Rate, this.Channels);
            this.ChannelHandle = BassMix.CreateMixerStream(this.Rate, this.Channels, this.Flags);
            if (this.ChannelHandle == 0)
            {
                BassUtils.Throw();
            }
            Logger.Write(this, LogLevel.Debug, "Adding stream to the mixer: {0}", previous.ChannelHandle);
            BassUtils.OK(BassMix.MixerAddChannel(this.ChannelHandle, previous.ChannelHandle, BassFlags.Default));
        }


        public override bool IsPlaying
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Playing;
            }
        }

        public override bool IsPaused
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Paused;
            }
        }

        public override bool IsStopped
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Stopped;
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
            Logger.Write(this, LogLevel.Debug, "Playing channel: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelPlay(this.ChannelHandle, true));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public override void Pause()
        {
            Logger.Write(this, LogLevel.Debug, "Pausing channel: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelPause(this.ChannelHandle));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public override void Resume()
        {
            Logger.Write(this, LogLevel.Debug, "Resuming channel: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelPlay(this.ChannelHandle, false));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public override void Stop()
        {
            Logger.Write(this, LogLevel.Debug, "Stopping channel: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelStop(this.ChannelHandle));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        protected override void OnDisposing()
        {
            if (this.ChannelHandle != 0)
            {
                Logger.Write(this, LogLevel.Debug, "Freeing BASS stream: {0}", this.ChannelHandle);
                BassUtils.OK(Bass.StreamFree(this.ChannelHandle));
            }
        }
    }
}
