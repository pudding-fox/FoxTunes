using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("B0D25121-4CB4-4D0C-9130-A67AF9D2AFEA", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_LOW)]
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassAsioStreamOutputBehaviour : StandardBehaviour, IConfigurableComponent, IInvocableComponent, IDisposable
    {
        public const string SETTINGS = "AAAA";

        public ICore Core { get; private set; }

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
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
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
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
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
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        private bool _Mixer { get; set; }

        public bool Mixer
        {
            get
            {
                return this._Mixer;
            }
            private set
            {
                this._Mixer = value;
                Logger.Write(this, LogLevel.Debug, "Mixer = {0}", this.Mixer);
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.OUTPUT_ELEMENT
            ).ConnectValue(value => this.Enabled = string.Equals(value.Id, BassAsioStreamOutputConfiguration.OUTPUT_ASIO_OPTION, StringComparison.OrdinalIgnoreCase));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassAsioStreamOutputConfiguration.ELEMENT_ASIO_DEVICE
            ).ConnectValue(value => this.AsioDevice = BassAsioStreamOutputConfiguration.GetAsioDevice(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassAsioStreamOutputConfiguration.DSD_RAW_ELEMENT
            ).ConnectValue(value => this.DsdDirect = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassAsioStreamOutputConfiguration.MIXER_ELEMENT
            ).ConnectValue(value => this.Mixer = value);
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            if (this.BassStreamPipelineFactory != null)
            {
                this.BassStreamPipelineFactory.QueryingPipeline += this.OnQueryingPipeline;
                this.BassStreamPipelineFactory.CreatingPipeline += this.OnCreatingPipeline;
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.IsInitialized = true;
            BassAsioUtils.OK(Bass.Configure(global::ManagedBass.Configuration.UpdateThreads, 0));
            BassAsioUtils.OK(Bass.Configure(global::ManagedBass.Configuration.PlaybackBufferLength, this.Output.BufferLength));
            BassAsioUtils.OK(Bass.Init(Bass.NoSoundDevice));
            //Always detect device for now.
            //if (BassAsioDevice.Info != null && BassAsioDevice.Info.Device != this.AsioDevice)
            {
                BassAsioDevice.Detect(this.AsioDevice);
            }
            Logger.Write(this, LogLevel.Debug, "BASS (No Sound) Initialized.");
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
            var output = new BassAsioStreamOutput(this, e.Stream);
            output.InitializeComponent(this.Core);
            e.Output = output;
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (BassAsioDevice.IsInitialized && BassAsioDevice.Info != null)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_SETTINGS, SETTINGS, "ASIO");
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case SETTINGS:
                    if (BassAsioDevice.IsInitialized && BassAsioDevice.Info != null)
                    {
                        BassAsioDevice.Info.ControlPanel();
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassAsioStreamOutputConfiguration.GetConfigurationSections();
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
            if (this.Output != null)
            {
                this.Output.Init -= this.OnInit;
                this.Output.Free -= this.OnFree;
            }
            if (this.BassStreamPipelineFactory != null)
            {
                this.BassStreamPipelineFactory.QueryingPipeline -= this.OnQueryingPipeline;
                this.BassStreamPipelineFactory.CreatingPipeline -= this.OnCreatingPipeline;
            }
        }

        ~BassAsioStreamOutputBehaviour()
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
