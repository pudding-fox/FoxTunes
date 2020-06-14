using FoxTunes.Interfaces;
using ManagedBass;
using System;

namespace FoxTunes
{
    public class BassSkipSilenceStreamComponent : BassStreamComponent
    {
        public BassSkipSilenceStreamComponent(BassSkipSilenceStreamAdvisorBehaviour behaviour, BassOutputStream stream)
        {
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
                return "SkipSilence";
            }
        }

        public override string Description
        {
            get
            {
                if (!this.IsActive)
                {
                    return string.Format("{0} (none)", this.Name);
                }
                var currentStream = this.PlaybackManager.CurrentStream as BassOutputStream;
                if (currentStream == null)
                {
                    return string.Format("{0} (none)", this.Name);
                }
                var leadIn = default(TimeSpan);
                var leadOut = default(TimeSpan);
                if (!BassSkipSilenceStreamAdvisor.TryGetMetaData(this.Behaviour, currentStream.PlaylistItem, out leadIn, out leadOut))
                {
                    return string.Format("{0} (none)", this.Name);
                }
                return string.Format(
                    "{0} ({1:0.00}s/{2:0.00}s)",
                    this.Name,
                    leadIn.TotalSeconds,
                    leadOut.TotalSeconds
                );
            }
        }

        public BassSkipSilenceStreamAdvisorBehaviour Behaviour { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public override bool IsActive
        {
            get
            {
                var currentStream = this.PlaybackManager.CurrentStream as BassOutputStream;
                if (currentStream == null)
                {
                    return false;
                }
                var leadIn = default(TimeSpan);
                var leadOut = default(TimeSpan);
                return BassSkipSilenceStreamAdvisor.TryGetMetaData(this.Behaviour, currentStream.PlaylistItem, out leadIn, out leadOut);
            }
        }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        public override void Connect(IBassStreamComponent previous)
        {
            this.Rate = previous.Rate;
            this.Channels = previous.Channels;
            this.ChannelHandle = previous.ChannelHandle;
        }

        protected override void OnDisposing()
        {
            //Nothing to do.
        }
    }
}
