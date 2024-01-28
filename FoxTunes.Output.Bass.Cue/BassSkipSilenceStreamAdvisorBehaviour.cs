using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassSkipSilenceStreamAdvisorBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public ICore Core { get; private set; }

        public IBassOutput Output { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IBassStreamPipelineFactory BassStreamPipelineFactory { get; private set; }

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
            this.Core = core;
            this.Output = ComponentRegistry.Instance.GetComponent<IBassOutput>();
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassSkipSilenceStreamAdvisorBehaviourConfiguration.ENABLED_ELEMENT
            ).ConnectValue(value => this.Enabled = value);
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassSkipSilenceStreamAdvisorBehaviourConfiguration.SENSITIVITY_ELEMENT
            ).ConnectValue(option => this.Threshold = BassSkipSilenceStreamAdvisorBehaviourConfiguration.GetSensitivity(option));
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            if (this.BassStreamPipelineFactory != null)
            {
                this.BassStreamPipelineFactory.CreatingPipeline += this.OnCreatingPipeline;
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            var component = new BassSkipSilenceStreamComponent(this, e.Stream);
            component.InitializeComponent(this.Core);
            e.Components.Add(component);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassSkipSilenceStreamAdvisorBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
