using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public class BassMasterChannel : BaseComponent, IDisposable
    {
        public const int CHANNELS = 2;

        public BassMasterChannel(BassOutput output)
        {
            this.Output = output;
        }

        public BassOutput Output { get; private set; }

        public int ChannelHandle { get; private set; }

        public bool IsStarted { get; private set; }

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

        public void StartStream(int rate)
        {
            if (this.ChannelHandle != 0)
            {
                this.FreeStream();
            }
            this.ChannelHandle = BASS_StreamCreateGaplessMaster(rate, CHANNELS, this.Flags, IntPtr.Zero);
            if (this.ChannelHandle == 0)
            {
                BassUtils.Throw();
            }
            Logger.Write(this, LogLevel.Debug, "Created master stream {0}/{1}: {2}.", rate, this.Output.Float ? "32F" : "16", this.ChannelHandle);
            this.OnStartedStream(rate);
            this.IsStarted = true;
        }

        protected virtual void OnStartedStream(int rate)
        {
            //Nothing to do;
        }

        public void FreeStream()
        {
            this.OnFreeingStream();
            Logger.Write(this, LogLevel.Debug, "Stopping master stream: {0}", this.ChannelHandle);
            Bass.StreamFree(this.ChannelHandle); //Not checking result code as it contains an error if the application is shutting down.
            this.ChannelHandle = 0;
            this.IsStarted = false;
        }

        protected virtual void OnFreeingStream()
        {
            //Nothing to do.
        }

        public virtual int GetPrimaryChannel()
        {
            return BASS_ChannelGetGaplessPrimary();
        }

        public virtual void SetPrimaryChannel(int channelHandle)
        {
            if (channelHandle != 0)
            {
                var channelRate = BassUtils.GetChannelRate(channelHandle);
                if (!this.IsStarted)
                {
                    this.StartStream(channelRate);
                }
                else
                {
                    var currentRate = BassUtils.GetChannelRate(this.ChannelHandle);
                    if (currentRate != channelRate)
                    {
                        Logger.Write(this, LogLevel.Warn, "Channel rate {0} differs from current rate {1}, restarting.", channelRate, currentRate);
                        this.StartStream(channelRate);
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
                var channelRate = BassUtils.GetChannelRate(channelHandle);
                var currentRate = BassUtils.GetChannelRate(this.ChannelHandle);
                if (currentRate != channelRate)
                {
                    Logger.Write(this, LogLevel.Warn, "Channel rate {0} differs from current rate {1}, cannot set secondary playback channel: {2}", channelRate, currentRate, channelHandle);
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
            this.FreeStream();
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
}
