using FoxTunes.Interfaces;
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

        private int _Threshold { get; set; }

        public int Threshold
        {
            get
            {
                return this._Threshold;
            }
            set
            {
                this._Threshold = value;
                Logger.Write(this, LogLevel.Debug, "Threshold = {0}", this.Threshold);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassSkipSilenceStreamAdvisorBehaviourConfiguration.ENABLED_ELEMENT
            ).ConnectValue(value => this.Enabled = value);
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassSkipSilenceStreamAdvisorBehaviourConfiguration.SENSITIVITY_ELEMENT
            ).ConnectValue(option => this.Threshold = BassSkipSilenceStreamAdvisorBehaviourConfiguration.GetSensitivity(option));
            base.InitializeComponent(core);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassSkipSilenceStreamAdvisorBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
