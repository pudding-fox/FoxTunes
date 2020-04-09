using FoxTunes.Interfaces;
using ManagedBass;
using System;

namespace FoxTunes
{
    public class BassReplayGainStreamComponent : BassStreamComponent
    {
        public BassReplayGainStreamComponent(BassReplayGainBehaviour behaviour, BassOutputStream stream)
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
                return "ReplayGain";
            }
        }

        public override string Description
        {
            get
            {
                var currentStream = this.PlaybackManager.CurrentStream as BassOutputStream;
                if (currentStream == null)
                {
                    return string.Format("{0} (none)", this.Name);
                }
                var effect = default(VolumeEffect);
                if (!this.Behaviour.Effects.TryGetValue(currentStream, out effect))
                {
                    return string.Format("{0} (none)", this.Name);
                }
                return string.Format(
                    "{0} ({1}{2}%/{3}dB/{4})",
                    this.Name,
                    effect.ReplayGain > 0 ? "+" : string.Empty,
                    Convert.ToInt32(effect.Volume * 100),
                    effect.ReplayGain,
                    Enum.GetName(typeof(ReplayGainMode), effect.Mode).ToLower()
                );
            }
        }

        public BassReplayGainBehaviour Behaviour { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        public override void Connect(IBassStreamComponent previous)
        {
            this.ChannelHandle = previous.ChannelHandle;
        }

        protected override void OnDisposing()
        {
            //Nothing to do.
        }
    }
}
