using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassSkipSilenceStreamAdvisorBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IConfiguration Configuration { get; private set; }

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
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassSkipSilenceStreamAdvisorBehaviourConfiguration.ENABLED_ELEMENT
            ).ConnectValue(value => this.Enabled = value);
            base.InitializeComponent(core);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassSkipSilenceStreamAdvisorBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
