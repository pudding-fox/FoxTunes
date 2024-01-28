using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassParametricEqualizerStreamComponent : BassStreamComponent
    {
        public const float MIN_BANDWIDTH = 0.5f;

        public const float MAX_BANDWIDTH = 5.0f;

        public const int MIN_GAIN = -15;

        public const int MAX_GAIN = 15;

        public BassParametricEqualizerStreamComponent(BassParametricEqualizerStreamComponentBehaviour behaviour, BassOutputStream stream)
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
            this.Attach();
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
                if (this.PeakEQ == null || !this.PeakEQ.Effects.Any())
                {
                    return string.Format("{0} (none)", this.Name);
                }
                var bands = string.Join(",", this.PeakEQ.Effects.Values.Select(effect => effect.Description));
                return string.Format(
                    "{0} ({1}@{2}octaves)",
                    this.Name,
                    bands,
                    this.Bandwidth
                );
            }
        }

        public BassParametricEqualizerStreamComponentBehaviour Behaviour { get; private set; }

        public float Bandwidth
        {
            get
            {
                var bandwidth = this.Behaviour.Configuration.GetElement<DoubleConfigurationElement>(
                    BassOutputConfiguration.SECTION,
                    BassParametricEqualizerStreamComponentConfiguration.BANDWIDTH
                ).Value;
                if (bandwidth < MIN_BANDWIDTH)
                {
                    bandwidth = MIN_BANDWIDTH;
                }
                else if (bandwidth > MAX_BANDWIDTH)
                {
                    bandwidth = MAX_BANDWIDTH;
                }
                return Convert.ToSingle(bandwidth);
            }
        }

        public PeakEQ PeakEQ { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        protected virtual void Attach()
        {
            this.Behaviour.Configuration.GetElement<DoubleConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassParametricEqualizerStreamComponentConfiguration.BANDWIDTH
            ).ValueChanged += this.OnBandwidthChanged;
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
                element.ValueChanged += this.OnBandChanged;
            }
        }

        protected virtual void Detach()
        {
            if (this.Behaviour == null || this.Behaviour.Configuration == null)
            {
                return;
            }
            this.Behaviour.Configuration.GetElement<IntegerConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassParametricEqualizerStreamComponentConfiguration.BANDWIDTH
            ).ValueChanged -= this.OnBandwidthChanged;
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
                element.ValueChanged -= this.OnBandChanged;
            }
        }

        public override void Connect(IBassStreamComponent previous)
        {
            this.Rate = previous.Rate;
            this.Channels = previous.Channels;
            this.ChannelHandle = previous.ChannelHandle;
            this.Start();
            this.Configure();
        }

        protected virtual void Start()
        {
            this.PeakEQ = new PeakEQ(this.ChannelHandle);
        }

        protected virtual void Stop()
        {
            this.PeakEQ.Dispose();
            this.PeakEQ = null;
        }

        protected virtual void Configure()
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
                this.Configure(band.Value, element.Value);
            }
        }

        protected virtual void Configure(int band, int gain)
        {
            if (gain != 0)
            {
                this.PeakEQ.AddOrUpdateBand(this.Bandwidth, band, gain);
            }
            else
            {
                this.PeakEQ.RemoveBand(band);
            }
        }

        protected virtual void OnBandwidthChanged(object sender, EventArgs e)
        {
            this.Configure();
        }

        protected virtual void OnBandChanged(object sender, EventArgs e)
        {
            var element = sender as IntegerConfigurationElement;
            if (element == null)
            {
                return;
            }
            foreach (var band in BassParametricEqualizerStreamComponentConfiguration.Bands)
            {
                if (!string.Equals(band.Key, element.Id))
                {
                    continue;
                }
                this.Configure(band.Value, element.Value);
                return;
            }
        }

        protected override void OnDisposing()
        {
            this.Detach();
            this.Stop();
        }
    }
}
