using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassOutputEqualizer : BassOutputEffect, IOutputEqualizer, IStandardComponent, IDisposable
    {
        public override bool Enabled
        {
            get
            {
                return this.EnabledElement.Value;
            }
            set
            {
                this.EnabledElement.Value = value;
                if (this.IsInitialized)
                {
                    this.Configuration.Save();
                }
            }
        }

        public IEnumerable<IOutputEqualizerBand> Bands { get; private set; }

        public IEnumerable<string> Presets { get; private set; }

        public void LoadPreset(string name)
        {
            BassParametricEqualizerStreamComponentConfiguration.LoadPreset(name);
        }

        public ICore Core { get; private set; }

        public BooleanConfigurationElement EnabledElement { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Core = core;
            this.EnabledElement = this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassParametricEqualizerStreamComponentConfiguration.ENABLED
            );
            this.EnabledElement.ValueChanged += this.OnEnabledChanged;
            this.Bands = new List<IOutputEqualizerBand>(this.GetBands());
            this.Presets = new List<string>(this.GetPresets());
        }

        protected virtual void OnEnabledChanged(object sender, EventArgs e)
        {
            this.OnEnabledChanged();
        }

        protected virtual IEnumerable<IOutputEqualizerBand> GetBands()
        {
            var bands = BassParametricEqualizerStreamComponentConfiguration.Bands.ToArray();
            for (var position = 0; position < bands.Length; position++)
            {
                var band = new BassOutputEqualizerBand(bands[position].Key, position, bands[position].Value);
                band.InitializeComponent(this.Core);
                yield return band;
            }
        }

        protected virtual IEnumerable<string> GetPresets()
        {
            return BassParametricEqualizerStreamComponentConfiguration.Presets.Select(
                pair => pair.Value
            );
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
            if (this.EnabledElement != null)
            {
                this.EnabledElement.ValueChanged -= this.OnEnabledChanged;
            }
        }

        ~BassOutputEqualizer()
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
