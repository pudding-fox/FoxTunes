using FoxTunes.Interfaces;
using ManagedBass.Sox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassResamplerStreamComponentBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IBassOutput Output { get; private set; }

        public IConfiguration Configuration { get; private set; }

        new public bool IsInitialized { get; private set; }

        private bool _Enabled { get; set; }

        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
                Logger.Write(this, LogLevel.Debug, "Enabled = {0}", this.Enabled);
                this.Output.Shutdown();
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassResamplerStreamComponentConfiguration.RESAMPLER_ELEMENT
            ).ConnectValue<bool>(value => this.Enabled = value);
            ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>().CreatingPipeline += this.OnCreatingPipeline;
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            BassUtils.OK(BassSox.Init());
            this.IsInitialized = true;
            Logger.Write(this, LogLevel.Debug, "BASS SOX Initialized.");
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Releasing BASS SOX.");
            BassSox.Free();
            this.IsInitialized = false;
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (!this.Enabled || this.Output.Rate == e.Stream.Rate)
            {
                return;
            }
            if (!this.Output.EnforceRate && e.Query.OutputRates.Contains(e.Stream.Rate))
            {
                return;
            }
            if (BassUtils.GetChannelDsdRaw(e.Stream.ChannelHandle))
            {
                return;
            }
            e.Components.Add(new BassResamplerStreamComponent(this, e.Stream));
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassResamplerStreamComponentConfiguration.GetConfigurationSections();
        }
    }
}
