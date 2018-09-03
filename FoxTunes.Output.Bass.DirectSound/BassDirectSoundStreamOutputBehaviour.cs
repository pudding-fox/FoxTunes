using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassDirectSoundStreamOutputBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IBassOutput Output { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IBassStreamPipelineFactory BassStreamPipelineFactory { get; private set; }

        public bool IsInitialized { get; private set; }

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

        private int _DirectSoundDevice { get; set; }

        public int DirectSoundDevice
        {
            get
            {
                return this._DirectSoundDevice;
            }
            set
            {
                this._DirectSoundDevice = value;
                Logger.Write(this, LogLevel.Debug, "Direct Sound Device = {0}", this.DirectSoundDevice);
                this.Output.Shutdown();
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.MODE_ELEMENT
            ).ConnectValue<string>(value => this.Enabled = string.Equals(value, BassDirectSoundStreamOutputConfiguration.MODE_DS_OPTION, StringComparison.OrdinalIgnoreCase));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassDirectSoundStreamOutputConfiguration.ELEMENT_DS_DEVICE
            ).ConnectValue<string>(value => this.DirectSoundDevice = BassDirectSoundStreamOutputConfiguration.GetDsDevice(value));
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            this.BassStreamPipelineFactory.QueryingPipeline += this.OnQueryingPipeline;
            this.BassStreamPipelineFactory.CreatingPipeline += this.OnCreatingPipeline;
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            BassUtils.OK(Bass.Configure(global::ManagedBass.Configuration.UpdateThreads, 1));
            BassUtils.OK(Bass.Init(this.DirectSoundDevice, this.Output.Rate));
            this.IsInitialized = true;
            Logger.Write(this, LogLevel.Debug, "BASS Initialized.");
        }

        protected virtual void OnInitDevice()
        {
            if (BassDirectSoundDevice.IsInitialized)
            {
                return;
            }
            BassDirectSoundDevice.Init(this.DirectSoundDevice);
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            if (!this.IsInitialized)
            {
                return;
            }
            this.OnFreeDevice();
            LogManager.Logger.Write(this, LogLevel.Debug, "Releasing BASS.");
            Bass.Free();
            this.IsInitialized = false;
        }

        protected virtual void OnFreeDevice()
        {
            if (!BassDirectSoundDevice.IsInitialized)
            {
                return;
            }
            BassDirectSoundDevice.Free();
        }

        protected virtual void OnQueryingPipeline(object sender, QueryingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.OnInitDevice();
            e.OutputRates = BassDirectSoundDevice.Info.SupportedRates;
            e.OutputChannels = BassDirectSoundDevice.Info.Outputs;
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            e.Output = new BassDirectSoundStreamOutput(this, e.Stream);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassDirectSoundStreamOutputConfiguration.GetConfigurationSections();
        }
    }
}
