using ManagedBass.Asio;
using ManagedBass.Gapless.Asio;

namespace FoxTunes
{
    public class BassAsioOutputChannel : BassOutputChannel
    {
        const int PRIMARY_CHANNEL = 0;

        public BassAsioOutputChannel(BassOutput output) : base(output)
        {

        }

        public AsioInitFlags AsioFlags
        {
            get
            {
                return AsioInitFlags.Thread;
            }
        }

        public AsioSampleFormat AsioFormat
        {
            get
            {
                if (this.Output.Float)
                {
                    return AsioSampleFormat.Float;
                }
                return AsioSampleFormat.Bit16;
            }
        }

        protected override void CreateChannel()
        {
            BassUtils.OK(BassAsio.Init(this.Output.AsioDevice, this.AsioFlags));
            BassUtils.OK(BassGaplessAsio.Init());
            BassUtils.OK(BassGaplessAsio.ChannelEnable(false, PRIMARY_CHANNEL));
            for (var channel = 1; channel < BassAsio.Info.Outputs; channel++)
            {
                BassUtils.OK(BassAsio.ChannelJoin(false, channel, PRIMARY_CHANNEL));
            }
            BassUtils.OK(BassAsio.ChannelSetFormat(false, PRIMARY_CHANNEL, this.AsioFormat));
            BassUtils.OK(BassAsio.ChannelSetRate(false, PRIMARY_CHANNEL, this.Rate));
            BassUtils.OK(BassAsio.Start(BassAsio.Info.PreferredBufferLength));
        }

        protected override void FreeChannel()
        {
            var flags =
                AsioChannelResetFlags.Enable |
                AsioChannelResetFlags.Join |
                AsioChannelResetFlags.Format |
                AsioChannelResetFlags.Rate;
            BassUtils.OK(BassAsio.Stop());
            for (var channel = 0; channel < BassAsio.Info.Outputs; channel++)
            {
                BassUtils.OK(BassAsio.ChannelReset(false, channel, flags));
            }
            BassUtils.OK(BassGaplessAsio.Free());
            BassUtils.OK(BassAsio.Free());
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
            BassUtils.OK(BassAsio.Start(BassAsio.Info.PreferredBufferLength));
        }

        public override void Pause()
        {
            BassUtils.OK(BassAsio.ChannelPause(false, PRIMARY_CHANNEL));
        }

        public override void Resume()
        {
            BassUtils.OK(BassAsio.ChannelReset(false, PRIMARY_CHANNEL, AsioChannelResetFlags.Pause));
        }

        public override void Stop()
        {
            if (!BassAsio.IsStarted)
            {
                return;
            }
            BassUtils.OK(BassAsio.Stop());
        }
    }
}
