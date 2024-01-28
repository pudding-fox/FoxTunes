using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassAsioStreamOutputBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IBassOutput Output { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IBassStreamPipelineFactory BassStreamPipelineFactory { get; private set; }

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

        private int _AsioDevice { get; set; }

        public int AsioDevice
        {
            get
            {
                return this._AsioDevice;
            }
            set
            {
                this._AsioDevice = value;
                Logger.Write(this, LogLevel.Debug, "ASIO Device = {0}", this.AsioDevice);
                this.Output.Shutdown();
            }
        }

        private bool _DsdDirect { get; set; }

        public bool DsdDirect
        {
            get
            {
                return this._DsdDirect;
            }
            private set
            {
                this._DsdDirect = value;
                Logger.Write(this, LogLevel.Debug, "DSD = {0}", this.DsdDirect);
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
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.MODE_ELEMENT
            ).ConnectValue<string>(value => this.Enabled = string.Equals(value, BassAsioStreamOutputConfiguration.MODE_ASIO_OPTION, StringComparison.OrdinalIgnoreCase));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassAsioStreamOutputConfiguration.ELEMENT_ASIO_DEVICE
            ).ConnectValue<string>(value => this.AsioDevice = BassAsioStreamOutputConfiguration.GetAsioDevice(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassAsioStreamOutputConfiguration.DSD_RAW_ELEMENT
            ).ConnectValue<bool>(value => this.DsdDirect = value);
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
            BassAsioUtils.OK(Bass.Configure(global::ManagedBass.Configuration.UpdateThreads, 0));
            BassAsioUtils.OK(Bass.Init(Bass.NoSoundDevice));
            this.IsInitialized = true;
            Logger.Write(this, LogLevel.Debug, "BASS (No Sound) Initialized.");
        }

        protected virtual void OnInitDevice()
        {
            if (BassAsioDevice.IsInitialized)
            {
                return;
            }
            BassAsioDevice.Init(this.AsioDevice);
            if (!BassAsioDevice.Info.SupportedRates.Contains(this.Output.Rate))
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
            if (!BassAsioDevice.IsInitialized)
            {
                return;
            }
            BassAsioDevice.Free();
        }

        protected virtual void OnQueryingPipeline(object sender, QueryingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.OnInitDevice();
            if (this.DsdDirect)
            {
                e.OutputCapabilities |= BassCapability.DSD_RAW;
            }
            e.OutputRates = BassAsioDevice.Info.SupportedRates;
            e.OutputChannels = BassAsioDevice.Info.Outputs;
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.OnInitDevice();
            e.Output = new BassAsioStreamOutput(this, e.Stream);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassAsioStreamOutputConfiguration.GetConfigurationSections();
        }
    }
}
