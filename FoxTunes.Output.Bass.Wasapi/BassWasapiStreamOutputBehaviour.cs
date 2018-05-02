using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassWasapiStreamOutputBehaviour : StandardBehaviour, IConfigurableComponent
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

        private int _WasapiDevice { get; set; }

        public int WasapiDevice
        {
            get
            {
                return this._WasapiDevice;
            }
            set
            {
                this._WasapiDevice = value;
                Logger.Write(this, LogLevel.Debug, "WASAPI Device = {0}", this.WasapiDevice);
                this.Output.Shutdown();
            }
        }

        private bool _Exclusive { get; set; }

        public bool Exclusive
        {
            get
            {
                return this._Exclusive;
            }
            private set
            {
                this._Exclusive = value;
                Logger.Write(this, LogLevel.Debug, "Exclusive = {0}", this.Exclusive);
                this.Output.Shutdown();
            }
        }

        private bool _EventDriven { get; set; }

        public bool EventDriven
        {
            get
            {
                return this._EventDriven;
            }
            private set
            {
                this._EventDriven = value;
                Logger.Write(this, LogLevel.Debug, "EventDriven = {0}", this.EventDriven);
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
            ).ConnectValue<string>(value => this.Enabled = string.Equals(value, BassWasapiStreamOutputConfiguration.MODE_WASAPI_OPTION, StringComparison.OrdinalIgnoreCase));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassWasapiStreamOutputConfiguration.ELEMENT_WASAPI_DEVICE
            ).ConnectValue<string>(value => this.WasapiDevice = BassWasapiStreamOutputConfiguration.GetWasapiDevice(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassWasapiStreamOutputConfiguration.ELEMENT_WASAPI_EXCLUSIVE
            ).ConnectValue<bool>(value => this.Exclusive = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassWasapiStreamOutputConfiguration.ELEMENT_WASAPI_EVENT
            ).ConnectValue<bool>(value => this.EventDriven = value);
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
            BassUtils.OK(Bass.Configure(global::ManagedBass.Configuration.UpdateThreads, 0));
            BassUtils.OK(Bass.Init(Bass.NoSoundDevice));
            this.IsInitialized = true;
            Logger.Write(this, LogLevel.Debug, "BASS (No Sound) Initialized.");
        }

        protected virtual void OnInitDevice()
        {
            if (BassWasapiDevice.IsInitialized)
            {
                return;
            }
            BassWasapiDevice.Init(this.WasapiDevice, this.Exclusive, this.EventDriven);
            if (!BassWasapiDevice.Info.SupportedRates.Contains(this.Output.Rate))
            {
                Logger.Write(this, LogLevel.Error, "The output rate {0} is not supported by the device.", this.Output.Rate);
                throw new NotImplementedException(string.Format("The output rate {0} is not supported by the device.", this.Output.Rate));
            }
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
            if (!BassWasapiDevice.IsInitialized)
            {
                return;
            }
            BassWasapiDevice.Free();
        }

        protected virtual void OnQueryingPipeline(object sender, QueryingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.OnInitDevice();
            e.OutputRates = BassWasapiDevice.Info.SupportedRates;
            e.OutputChannels = BassWasapiDevice.Info.Outputs;
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.OnInitDevice();
            e.Output = new BassWasapiStreamOutput(this, e.Stream);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassWasapiStreamOutputConfiguration.GetConfigurationSections();
        }
    }
}
