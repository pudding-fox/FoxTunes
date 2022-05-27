using FoxTunes.Interfaces;
using ManagedBass.Dsd;
using ManagedBass.Memory;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    //This component does not technically require an output but we don't want to present anything else with DSD data.
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassDsdBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public ICore Core { get; private set; }

        public IBassOutput Output { get; private set; }

        public IBassStreamPipelineFactory BassStreamPipelineFactory { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public int Rate
        {
            get
            {
                return BassDsd.DefaultFrequency;
            }
            set
            {
                BassDsd.DefaultFrequency = value;
                Logger.Write(this, LogLevel.Debug, "DSD to PCM sample rate: {0}", MetaDataInfo.SampleRateDescription(BassDsd.DefaultFrequency));
            }
        }

        public int Gain
        {
            get
            {
                return BassDsd.DefaultGain;
            }
            set
            {
                BassDsd.DefaultGain = value;
                Logger.Write(this, LogLevel.Debug, "DSD to PCM gain: {0}{1}dB", BassDsd.DefaultGain > 0 ? "+" : string.Empty, BassDsd.DefaultGain);
            }
        }

        private bool _Memory { get; set; }

        public bool Memory
        {
            get
            {
                return this._Memory;
            }
            set
            {
                this._Memory = value;
                Logger.Write(this, LogLevel.Debug, "Play DSD from memory: {0}", value ? bool.TrueString : bool.FalseString);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            this.BassStreamPipelineFactory.CreatingPipeline += this.OnCreatingPipeline;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassDsdBehaviourConfiguration.DSD_RATE_ELEMENT
            ).ConnectValue(value => this.Rate = BassDsdBehaviourConfiguration.GetRate(value));
            this.Configuration.GetElement<IntegerConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassDsdBehaviourConfiguration.DSD_GAIN_ELEMENT
            ).ConnectValue(value => this.Gain = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassDsdBehaviourConfiguration.DSD_MEMORY_ELEMENT
            ).ConnectValue(value => this.Memory = value);
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            if (!this.Memory)
            {
                return;
            }
            BassMemory.Dsd.Init();
            Logger.Write(this, LogLevel.Debug, "BASS MEMORY DSD Initialized.");
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            if (!this.Memory)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Releasing BASS DSD MEMORY.");
            BassMemory.Dsd.Free();
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (!BassDsdMemoryStreamComponent.ShouldCreate(this, e.Stream, e.Query))
            {
                return;
            }
            var component = new BassDsdMemoryStreamComponent(this, e.Stream.Flags);
            component.InitializeComponent(this.Core);
            e.Components.Add(component);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassDsdBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
