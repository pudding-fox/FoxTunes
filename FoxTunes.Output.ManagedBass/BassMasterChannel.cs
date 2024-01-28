using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public class BassMasterChannel : BaseComponent, IDisposable
    {
        public BassMasterChannel(BassOutput output)
        {
            this.Output = output;
        }

        public BassOutput Output { get; private set; }

        public int ChannelHandle { get; private set; }

        public BassMasterChannelConfig Config { get; private set; }

        public bool IsStarted { get; protected set; }

        public virtual BassFlags Flags
        {
            get
            {
                var flags = BassFlags.Default;
                if (this.Output.Float)
                {
                    flags |= BassFlags.Float;
                }
                return flags;
            }
        }

        protected virtual void StartStream(BassMasterChannelConfig config)
        {
            this.StopStream();
            this.Config = config;
            this.ChannelHandle = BASS_StreamCreateGaplessMaster(config.EffectiveRate, config.Channels, this.Flags, IntPtr.Zero);
            if (this.ChannelHandle == 0)
            {
                BassUtils.Throw();
            }
            Logger.Write(this, LogLevel.Debug, "Created master stream {0}/{1}: {2}.", config.EffectiveRate, this.Output.Float ? "Float" : "16", this.ChannelHandle);
            this.IsStarted = true;
        }

        protected virtual void StopStream()
        {
            if (!this.IsStarted)
            {
                return;
            }
            try
            {
                Logger.Write(this, LogLevel.Debug, "Stopping master stream: {0}", this.ChannelHandle);
                Bass.StreamFree(this.ChannelHandle); //Not checking result code as it contains an error if the application is shutting down.
                this.ChannelHandle = 0;
            }
            finally
            {
                this.IsStarted = false;
            }
        }

        public virtual int GetPrimaryChannel()
        {
            return BASS_ChannelGetGaplessPrimary();
        }

        public virtual void SetPrimaryChannel(int channelHandle)
        {
            if (channelHandle != 0)
            {
                var config = new BassMasterChannelConfig(this.Output, channelHandle);
                if (!this.IsStarted)
                {
                    this.StartStream(config);
                }
                else
                {
                    if (this.Config != config)
                    {
                        Logger.Write(this, LogLevel.Warn, "Channel config \"{0}\" differs from current config \"{1}\", restarting.", config, this.Config);
                        this.StartStream(config);
                    }
                }
            }
            Logger.Write(this, LogLevel.Debug, "Setting primary playback channel: {0}", channelHandle);
            BassUtils.OK(BASS_ChannelSetGaplessPrimary(channelHandle));
        }

        public int GetSecondaryChannel()
        {
            return BASS_ChannelGetGaplessSecondary();
        }

        public void SetSecondaryChannel(int channelHandle)
        {
            if (channelHandle != 0)
            {
                if (!this.IsStarted)
                {
                    Logger.Write(this, LogLevel.Warn, "Not yet started, cannot set secondary playback channel: {0}", channelHandle);
                    return;
                }
                var config = new BassMasterChannelConfig(this.Output, channelHandle);
                if (this.Config != config)
                {
                    Logger.Write(this, LogLevel.Warn, "Channel config \"{0}\" differs from current config \"{1}\", cannot set secondary playback channel: {2}", config, this.Config, channelHandle);
                    return;
                }
            }
            Logger.Write(this, LogLevel.Debug, "Setting secondary playback channel: {0}", channelHandle);
            BassUtils.OK(BASS_ChannelSetGaplessSecondary(channelHandle));
        }

        public virtual bool IsPlaying
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Playing;
            }
        }

        public virtual bool IsPaused
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Paused;
            }
        }

        public virtual bool IsStopped
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Stopped;
            }
        }

        public virtual void Play()
        {
            Logger.Write(this, LogLevel.Debug, "Starting master stream: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelPlay(this.ChannelHandle));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public virtual void Pause()
        {
            Logger.Write(this, LogLevel.Debug, "Pausing master stream: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelPause(this.ChannelHandle));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public virtual void Resume()
        {
            Logger.Write(this, LogLevel.Debug, "Resuming master stream: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelPlay(this.ChannelHandle));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public virtual void Stop()
        {
            Logger.Write(this, LogLevel.Debug, "Stopping master stream: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelStop(this.ChannelHandle));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            this.StopStream();
        }

        ~BassMasterChannel()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }

        [DllImport("bass_foxtunes.dll", EntryPoint = "BASS_StreamCreateGaplessMaster")]
        static extern int BASS_StreamCreateGaplessMaster(int Frequency, int Channels, BassFlags Flags, IntPtr User);

        [DllImport("bass_foxtunes.dll", EntryPoint = "BASS_ChannelGetGaplessPrimary")]
        static extern int BASS_ChannelGetGaplessPrimary();

        [DllImport("bass_foxtunes.dll", EntryPoint = "BASS_ChannelSetGaplessPrimary")]
        static extern int BASS_ChannelSetGaplessPrimary(int Channel);

        [DllImport("bass_foxtunes.dll", EntryPoint = "BASS_ChannelGetGaplessSecondary")]
        static extern int BASS_ChannelGetGaplessSecondary();

        [DllImport("bass_foxtunes.dll", EntryPoint = "BASS_ChannelSetGaplessSecondary")]
        static extern int BASS_ChannelSetGaplessSecondary(int Channel);
    }

    public class BassMasterChannelConfig : IEquatable<BassMasterChannelConfig>
    {
        public BassMasterChannelConfig(BassOutput output, int channelHandle)
        {
            this.DsdDirect = output.DsdDirect;
            this.PcmRate = BassUtils.GetChannelPcmRate(channelHandle);
            this.DsdRate = BassUtils.GetChannelDsdRate(channelHandle);
            this.Channels = BassUtils.GetChannelCount(channelHandle);
        }

        public bool DsdDirect { get; private set; }

        public int PcmRate { get; private set; }

        public int DsdRate { get; private set; }

        public int Channels { get; private set; }

        public int EffectiveRate
        {
            get
            {
                if (this.DsdDirect && this.DsdRate > 0)
                {
                    return this.DsdRate;
                }
                return this.PcmRate;
            }
        }

        public override string ToString()
        {
            return string.Format("DSD Direct {0}, PCM Rate {1}, DSD Rate {2}, Channels {3}", this.DsdDirect, this.PcmRate, this.DsdRate, this.Channels);
        }

        public bool Equals(BassMasterChannelConfig other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            return this.GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BassMasterChannelConfig);
        }

        public override int GetHashCode()
        {
            var hashCode = default(int);
            hashCode += this.EffectiveRate.GetHashCode();
            hashCode += this.Channels.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(BassMasterChannelConfig a, BassMasterChannelConfig b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            if (object.ReferenceEquals((object)a, (object)b))
            {
                return true;
            }
            return a.Equals(b);
        }

        public static bool operator !=(BassMasterChannelConfig a, BassMasterChannelConfig b)
        {
            return !(a == b);
        }
    }
}
