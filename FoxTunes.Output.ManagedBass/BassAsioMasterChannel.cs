using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Asio;
using System;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public class BassAsioMasterChannel : BassMasterChannel
    {
        const int PRIMARY_CHANNEL = 0;

        public BassAsioMasterChannel(BassOutput output) : base(output)
        {

        }

        public override BassFlags Flags
        {
            get
            {
                return base.Flags | BassFlags.Decode;
            }
        }

        protected override void OnStartedStream(int rate)
        {
            BassUtils.OK(BASS_ASIO_ChannelEnableGaplessMaster(false, PRIMARY_CHANNEL, new IntPtr(this.ChannelHandle)));
            for (var a = 1; a < CHANNELS; a++)
            {
                BassUtils.OK(BassAsio.ChannelJoin(false, a, PRIMARY_CHANNEL));
            }
            BassUtils.OK(BassAsio.ChannelSetFormat(false, PRIMARY_CHANNEL, AsioSampleFormat.Bit16));
            BassUtils.OK(BassAsio.ChannelSetRate(false, PRIMARY_CHANNEL, rate));
            Logger.Write(this, LogLevel.Debug, "Enabled ASIO on master stream: {0}", this.ChannelHandle);
            base.OnStartedStream(rate);
        }

        protected override void OnFreeingStream()
        {
            if (BassAsio.IsStarted)
            {
                BassUtils.OK(BassAsio.Stop());
            }
            BassUtils.OK(BassAsio.ChannelReset(false, PRIMARY_CHANNEL, AsioChannelResetFlags.Enable));
            for (var a = 1; a < CHANNELS; a++)
            {
                BassUtils.OK(BassAsio.ChannelReset(false, a, AsioChannelResetFlags.Join));
            }
            Logger.Write(this, LogLevel.Debug, "Disabled ASIO on master stream: {0}", this.ChannelHandle);
            base.OnFreeingStream();
        }

        public override void SetPrimaryChannel(int channelHandle)
        {
            base.SetPrimaryChannel(channelHandle);
            if (!BassAsio.IsStarted)
            {
                BassAsio.Start();
            }
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
                return BassAsio.ChannelIsActive(false, PRIMARY_CHANNEL) == AsioChannelActive.Paused;
            }
        }

        public override bool IsStopped
        {
            get
            {
                return !BassAsio.IsStarted;
            }
        }

        public override void Play()
        {
            if (BassAsio.IsStarted)
            {
                return;
            }
            BassAsio.Start();
        }

        public override void Pause()
        {
            BassUtils.OK(BassAsio.ChannelPause(false, PRIMARY_CHANNEL));
        }

        public override void Resume()
        {
            BassAsio.ChannelReset(false, PRIMARY_CHANNEL, AsioChannelResetFlags.Pause);
        }

        public override void Stop()
        {
            if (!BassAsio.IsStarted)
            {
                return;
            }
            BassAsio.Stop();
        }

        [DllImport("bass_foxtunes.dll", EntryPoint = "BASS_ASIO_ChannelEnableGaplessMaster")]
        static extern int BASS_ASIO_ChannelEnableGaplessMaster(bool Input, int Channel, IntPtr User);
    }
}
