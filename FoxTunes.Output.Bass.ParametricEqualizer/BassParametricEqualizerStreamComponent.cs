using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.DirectX8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxTunes
{
    public class BassParametricEqualizerStreamComponent : BassStreamComponent
    {
        public const int MIN_BANDWIDTH = 1;

        public const int MAX_BANDWIDTH = 36;

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
                var bands = string.Join(",", this.Bands.Where(band => band.Gain != 0).Select(band => band.Description));
                if (string.IsNullOrEmpty(bands))
                {
                    bands = "None";
                }
                return string.Format(
                    "{0} ({1}@{2}semitones)",
                    this.Name,
                    bands,
                    this.Bandwidth
                );
            }
        }

        public BassParametricEqualizerStreamComponentBehaviour Behaviour { get; private set; }

        public int Bandwidth
        {
            get
            {
                var bandwidth = this.Behaviour.Configuration.GetElement<IntegerConfigurationElement>(
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
                return bandwidth;
            }
        }

        public List<Band> Bands { get; private set; }

        public DXParamEQ Eq { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        protected virtual void Attach()
        {
            this.Behaviour.Configuration.GetElement<IntegerConfigurationElement>(
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
            Logger.Write(this, LogLevel.Debug, "Creating BASS PARAMETRIC EQUALIZER stream with rate {0} => {1} and {2} channels.", previous.Rate, this.Rate, this.Channels);
            this.ChannelHandle = previous.ChannelHandle;
            this.Start();
            this.Configure();
        }

        protected virtual void Start()
        {
            Logger.Write(this, LogLevel.Debug, "Creating DX8 ParamEQ Effect: Bandwidth = {0}", this.Bandwidth);
            this.Eq = new DXParamEQ(this.ChannelHandle, this.Bandwidth);
            Logger.Write(this, LogLevel.Debug, "Creating DX8 ParamEQ Bands.");
            this.Bands = BassParametricEqualizerStreamComponentConfiguration.Bands.Select(
                band => new Band(this.Behaviour, band.Key, this.Eq.AddBand(band.Value), band.Value)
            ).ToList();
        }

        protected virtual void Stop()
        {
            if (this.Eq != null)
            {
                this.Eq.Dispose();
                this.Eq = null;
            }
            if (this.Bands != null)
            {
                this.Bands = null;
            }
        }

        protected virtual void Configure()
        {
            foreach (var band in this.Bands)
            {
                Logger.Write(this, LogLevel.Debug, "Updating DX8 ParamEQ Band: {0} => {1} => {2}", band.Id, band.Center, band.Gain);
                this.Eq.UpdateBand(band.Index, band.Gain);
            }
        }

        protected virtual void OnBandwidthChanged(object sender, EventArgs e)
        {
            this.Stop();
            this.Start();
            this.Configure();
        }

        protected virtual void OnBandChanged(object sender, EventArgs e)
        {
            this.Configure();
        }

        protected override void OnDisposing()
        {
            this.Detach();
            this.Stop();
        }

        public class Band
        {
            public Band(BassParametricEqualizerStreamComponentBehaviour behaviour, string id, int index, int center)
            {
                this.Behaviour = behaviour;
                this.Id = id;
                this.Index = index;
                this.Center = center;
            }

            public BassParametricEqualizerStreamComponentBehaviour Behaviour { get; private set; }

            public string Id { get; private set; }

            public int Index { get; private set; }

            public int Center { get; private set; }

            public int Gain
            {
                get
                {
                    var gain = this.Behaviour.Configuration.GetElement<IntegerConfigurationElement>(
                        BassOutputConfiguration.SECTION,
                        this.Id
                    ).Value;
                    if (gain < MIN_GAIN)
                    {
                        gain = MIN_GAIN;
                    }
                    else if (gain > MAX_GAIN)
                    {
                        gain = MAX_GAIN;
                    }
                    return gain;
                }
            }

            public string Description
            {
                get
                {
                    return string.Format(
                        "{0}/{1}dB",
                        GetBandName(this.Center),
                        this.Gain > 0 ? "+" + this.Gain.ToString() : this.Gain.ToString()
                    );
                }
            }

            public static string GetBandName(int value)
            {
                if (value < 1000)
                {
                    return string.Format("{0}Hz", value);
                }
                else
                {
                    return string.Format("{0}kHz", value / 1000);
                }
            }
        }
    }
}
