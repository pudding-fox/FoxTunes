using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class PeakEQ : BaseComponent, IDisposable
    {
        public const int MIN_BANDWIDTH = 1;

        public const int MAX_BANDWIDTH = 36;

        public PeakEQ()
        {
            this.Effects = new Dictionary<float, PeakEQEffect>();
        }

        public PeakEQ(int channelHandle) : this()
        {
            this.ChannelHandle = channelHandle;
        }

        public IDictionary<float, PeakEQEffect> Effects { get; private set; }

        public int ChannelHandle { get; private set; }

        public PeakEQEffect AddOrUpdateBand(float bandwidth, float center, float gain)
        {
            var effect = default(PeakEQEffect);
            if (!this.Effects.TryGetValue(center, out effect))
            {
                effect = new PeakEQEffect(this.ChannelHandle);
                effect.Band = this.Effects.Count;
                effect.Center = center;
                effect.Channel = -1;
                this.Effects.Add(center, effect);
            }
            effect.Bandwidth = bandwidth;
            effect.Gain = gain;
            effect.Activate();
            return effect;
        }

        public void RemoveBand(float center)
        {
            var effect = default(PeakEQEffect);
            if (!this.Effects.TryGetValue(center, out effect))
            {
                return;
            }
            effect.Dispose();
            this.Effects.Remove(center);
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
            foreach (var key in this.Effects.Keys)
            {
                var effect = this.Effects[key];
                effect.Dispose();
            }
            this.Effects.Clear();
        }

        ~PeakEQ()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
