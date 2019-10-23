using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.DirectX8;
using ManagedBass.Fx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassParametricEqualizerStreamComponent : BassStreamComponent
    {
        public const int MIN_GAIN = -15;

        public const int MAX_GAIN = 15;

        public BassParametricEqualizerStreamComponent(BassParametricEqualizerStreamComponentBehaviour behaviour, BassOutputStream stream)
        {
            if (BassUtils.GetChannelDsdRaw(stream.ChannelHandle))
            {
                throw new InvalidOperationException("Cannot apply effects to DSD streams.");
            }
            this.Behaviour = behaviour;
            foreach (var band in BassParametricEqualizerStreamComponentConfiguration.Bands)
            {
                var element = this.Behaviour.Configuration.GetElement<IntegerConfigurationElement>(
                    BassOutputConfiguration.SECTION,
                    band.Key
                );
                if (element == null)
                {
                    continue;
                }
                element.ValueChanged += this.OnValueChanged;
            }
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
                return "ParametricEqualizer";
            }
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    "{0} ({1}/{2}/{3})",
                    this.Name,
                    BassUtils.DepthDescription(this.Flags),
                    MetaDataInfo.SampleRateDescription(this.Rate),
                    MetaDataInfo.ChannelDescription(this.Channels)
                );
            }
        }

        public BassParametricEqualizerStreamComponentBehaviour Behaviour { get; private set; }

        public List<Band> Bands { get; private set; }

        public DXParamEQ Eq { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public override void Connect(IBassStreamComponent previous)
        {
            Logger.Write(this, LogLevel.Debug, "Creating BASS PARAMETRIC EQUALIZER stream with rate {0} => {1} and {2} channels.", previous.Rate, this.Rate, this.Channels);
            this.ChannelHandle = previous.ChannelHandle;
            this.Configure();
        }

        protected virtual void Configure()
        {
            if (this.Eq == null)
            {
                this.Eq = new DXParamEQ(this.ChannelHandle);
            }
            if (this.Bands == null)
            {
                this.Bands = BassParametricEqualizerStreamComponentConfiguration.Bands.Select(
                    band => new Band(band.Key, this.Eq.AddBand(band.Value), band.Value)
                ).ToList();
            }
            foreach (var band in this.Bands)
            {
                var gain = this.Behaviour.Configuration.GetElement<IntegerConfigurationElement>(
                    BassOutputConfiguration.SECTION,
                    band.Id
                ).Value;
                if (gain < MIN_GAIN)
                {
                    gain = MIN_GAIN;
                }
                else if (gain > MAX_GAIN)
                {
                    gain = MAX_GAIN;
                }
                this.Eq.UpdateBand(band.Index, gain);
            }
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.Configure();
        }

        protected override void OnDisposing()
        {
            if (this.Behaviour != null && this.Behaviour.Configuration != null)
            {
                foreach (var band in BassParametricEqualizerStreamComponentConfiguration.Bands)
                {
                    var element = this.Behaviour.Configuration.GetElement<IntegerConfigurationElement>(
                        BassOutputConfiguration.SECTION,
                        band.Key
                    );
                    if (element == null)
                    {
                        continue;
                    }
                    element.ValueChanged -= this.OnValueChanged;
                }
            }
            if (this.Eq != null)
            {
                this.Eq.Dispose();
                this.Eq = null;
            }
        }

        public class Band
        {
            public Band(string id, int index, float center)
            {
                this.Id = id;
                this.Index = index;
                this.Center = center;
            }

            public string Id { get; private set; }

            public int Index { get; private set; }

            public float Center { get; private set; }
        }
    }
}
