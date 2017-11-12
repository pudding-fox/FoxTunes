using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public class BassMasterChannel : BaseComponent, IDisposable
    {
        const int RATE = 44100;

        const int CHANNELS = 2;

        public BassMasterChannel(BassOutput output)
        {
            this.Output = output;
        }

        public BassOutput Output { get; private set; }

        public int ChannelHandle { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ChannelHandle = BASS_StreamCreateGaplessMaster(RATE, CHANNELS, BassFlags.Default, IntPtr.Zero);
            Logger.Write(this, LogLevel.Debug, "Created master stream: {0}.", this.ChannelHandle);
            base.InitializeComponent(core);
        }

        public void SetPrimaryChannel(int channelHandle)
        {
            Logger.Write(this, LogLevel.Debug, "Setting primary playback channel: {0}", channelHandle);
            BASS_ChannelSetGaplessPrimary(channelHandle);
        }

        public void SetSecondaryChannelHandle(int channelHandle)
        {
            Logger.Write(this, LogLevel.Debug, "Setting secondary playback channel: {0}", channelHandle);
            BASS_ChannelSetGaplessSecondary(channelHandle);
        }

        public bool IsPlaying
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Playing;
            }
        }

        public bool IsPaused
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Paused;
            }
        }

        public bool IsStopped
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Stopped;
            }
        }

        public void Play()
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

        public void Pause()
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

        public void Resume()
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

        public void Stop()
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
            Bass.StreamFree(this.ChannelHandle); //Not checking result code as it contains an error if the application is shutting down.
            this.ChannelHandle = 0;
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
