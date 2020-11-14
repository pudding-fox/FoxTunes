using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Fx;
using System;

namespace FoxTunes
{
    public class BassOutputTempoStreamComponent : BassStreamComponent
    {
        public BassOutputTempoStreamComponent(BassOutputTempoStreamComponentBehaviour behaviour, BassOutputStream stream)
        {
            if (BassUtils.GetChannelDsdRaw(stream.ChannelHandle))
            {
                throw new InvalidOperationException("Cannot apply effects to DSD streams.");
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
                return "Tempo";
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
                return string.Empty;
            }
        }

        public BassOutputTempoStreamComponentBehaviour Behaviour { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public override bool IsActive
        {
            get
            {
                if (this.OutputEffects == null || this.OutputEffects.Tempo == null)
                {
                    return false;
                }
                if (!this.OutputEffects.Tempo.Available || !this.OutputEffects.Tempo.Enabled)
                {
                    return false;
                }
                return true;
            }
        }

        public IOutputEffects OutputEffects { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.OutputEffects = core.Components.OutputEffects;
            if (this.OutputEffects.Tempo != null)
            {
                this.OutputEffects.Tempo.EnabledChanged += this.OnEnabledChanged;
                this.OutputEffects.Tempo.TempoChanged += this.OnTempoChanged;
                this.OutputEffects.Tempo.PitchChanged += this.OnPitchChanged;
                this.OutputEffects.Tempo.RateChanged += this.OnRateChanged;
                this.OutputEffects.Tempo.AAFilterChanged += this.OnAAFilterChanged;
                this.OutputEffects.Tempo.AAFilterLengthChanged += this.OnAAFilterLengthChanged;
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnEnabledChanged(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                this.Update();
            }
            else
            {
                this.Stop();
            }
        }

        protected virtual void OnTempoChanged(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                this.Update();
            }
        }

        protected virtual void OnPitchChanged(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                this.Update();
            }
        }

        protected virtual void OnRateChanged(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                this.Update();
            }
        }

        protected virtual void OnAAFilterChanged(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                this.Update();
            }
        }

        protected virtual void OnAAFilterLengthChanged(object sender, EventArgs e)
        {
            if (this.IsActive)
            {
                this.Update();
            }
        }

        protected virtual void Update()
        {
            BassUtils.OK(Bass.ChannelSetAttribute(this.ChannelHandle, ChannelAttribute.Tempo, this.OutputEffects.Tempo.Tempo));
            BassUtils.OK(Bass.ChannelSetAttribute(this.ChannelHandle, ChannelAttribute.Pitch, this.OutputEffects.Tempo.Pitch));
            BassUtils.OK(Bass.ChannelSetAttribute(this.ChannelHandle, ChannelAttribute.TempoFrequency, 1.0 + ((double)this.OutputEffects.Tempo.Rate / Tempo.MAX_RATE)));
            BassUtils.OK(Bass.ChannelSetAttribute(this.ChannelHandle, ChannelAttribute.TempoUseAAFilter, this.OutputEffects.Tempo.AAFilter ? 1 : 0));
            BassUtils.OK(Bass.ChannelSetAttribute(this.ChannelHandle, ChannelAttribute.TempoAAFilterLength, this.OutputEffects.Tempo.AAFilterLength));
        }

        protected virtual void Stop()
        {
            BassUtils.OK(Bass.ChannelSetAttribute(this.ChannelHandle, ChannelAttribute.Tempo, 0));
            BassUtils.OK(Bass.ChannelSetAttribute(this.ChannelHandle, ChannelAttribute.Pitch, 0));
            BassUtils.OK(Bass.ChannelSetAttribute(this.ChannelHandle, ChannelAttribute.TempoFrequency, 0));
            BassUtils.OK(Bass.ChannelSetAttribute(this.ChannelHandle, ChannelAttribute.TempoUseAAFilter, 1));
            BassUtils.OK(Bass.ChannelSetAttribute(this.ChannelHandle, ChannelAttribute.TempoAAFilterLength, 32));
        }

        public override void Connect(IBassStreamComponent previous)
        {
            this.Rate = previous.Rate;
            this.Channels = previous.Channels;
            this.ChannelHandle = BassFx.TempoCreate(previous.ChannelHandle, previous.Flags);
            if (this.ChannelHandle == 0)
            {
                BassUtils.Throw();
            }
            if (this.OutputEffects.Tempo.Enabled)
            {
                this.Update();
            }
        }

        protected override void OnDisposing()
        {
            if (this.OutputEffects != null && this.OutputEffects.Tempo != null)
            {
                this.OutputEffects.Tempo.EnabledChanged -= this.OnEnabledChanged;
                this.OutputEffects.Tempo.TempoChanged -= this.OnTempoChanged;
                this.OutputEffects.Tempo.PitchChanged -= this.OnPitchChanged;
                this.OutputEffects.Tempo.RateChanged -= this.OnRateChanged;
                this.OutputEffects.Tempo.AAFilterChanged -= this.OnAAFilterChanged;
                this.OutputEffects.Tempo.AAFilterLengthChanged -= this.OnAAFilterLengthChanged;
            }
            this.Stop();
        }
    }
}
