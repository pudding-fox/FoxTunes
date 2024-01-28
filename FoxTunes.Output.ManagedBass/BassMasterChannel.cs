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

        public BassMasterChannel()
        {

        }

        public int ChannelHandle { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ChannelHandle = BASS_StreamCreateGaplessMaster(RATE, CHANNELS, BassFlags.Default, IntPtr.Zero);
            BassUtils.OK(Bass.ChannelPlay(this.ChannelHandle));
            base.InitializeComponent(core);
        }

        public void SetPrimaryChannel(int channelhandle)
        {
            BASS_ChannelSetGaplessPrimary(channelhandle);
        }

        public void SetStandbyChannelHandle(int channelHandle)
        {
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
            BassUtils.OK(Bass.ChannelPlay(this.ChannelHandle));
        }

        public void Pause()
        {
            BassUtils.OK(Bass.ChannelPause(this.ChannelHandle));
        }

        public void Resume()
        {
            BassUtils.OK(Bass.ChannelPlay(this.ChannelHandle));
        }

        public void Stop()
        {
            BassUtils.OK(Bass.ChannelStop(this.ChannelHandle));
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
