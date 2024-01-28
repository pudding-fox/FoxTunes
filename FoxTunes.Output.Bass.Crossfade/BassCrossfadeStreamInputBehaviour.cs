using FoxTunes.Interfaces;
using ManagedBass.Crossfade;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassCrossfadeStreamInputBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        static BassCrossfadeStreamInputBehaviour()
        {
            BassPluginLoader.Instance.Load();
        }

        public BassCrossfadeStreamInputBehaviour()
        {
            BassUtils.OK(BassCrossfade.Init());
            Logger.Write(this, LogLevel.Debug, "BASS CROSSFADE Initialized.");
        }

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
                //TODO: Bad .Wait().
                this.Output.Shutdown().Wait();
            }
        }

        public BassCrossfadeMode Mode
        {
            get
            {
                return BassCrossfade.Mode;
            }
            set
            {
                BassCrossfade.Mode = value;
                Logger.Write(this, LogLevel.Debug, "Mode = {0}", Enum.GetName(typeof(BassCrossfadeMode), this.Mode));
            }
        }

        public int InPeriod
        {
            get
            {
                return BassCrossfade.InPeriod;
            }
            set
            {
                BassCrossfade.InPeriod = value;
                Logger.Write(this, LogLevel.Debug, "InPeriod = {0}", this.InPeriod);
            }
        }

        public int OutPeriod
        {
            get
            {
                return BassCrossfade.OutPeriod;
            }
            set
            {
                BassCrossfade.OutPeriod = value;
                Logger.Write(this, LogLevel.Debug, "OutPeriod = {0}", this.OutPeriod);
            }
        }

        public BassCrossfadeType InType
        {
            get
            {
                return BassCrossfade.InType;
            }
            set
            {
                BassCrossfade.InType = value;
                Logger.Write(this, LogLevel.Debug, "InType = {0}", Enum.GetName(typeof(BassCrossfadeType), this.InType));
            }
        }

        public BassCrossfadeType OutType
        {
            get
            {
                return BassCrossfade.OutType;
            }
            set
            {
                BassCrossfade.OutType = value;
                Logger.Write(this, LogLevel.Debug, "OutType = {0}", Enum.GetName(typeof(BassCrossfadeType), this.OutType));
            }
        }

        public bool Mix
        {
            get
            {
                return BassCrossfade.Mix;
            }
            set
            {
                BassCrossfade.Mix = value;
                Logger.Write(this, LogLevel.Debug, "Mix = {0}", this.Mix);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output as IBassOutput;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.INPUT_ELEMENT
            ).ConnectValue(value => this.Enabled = string.Equals(value.Id, BassCrossfadeStreamInputConfiguration.INPUT_CROSSFADE_OPTION, StringComparison.OrdinalIgnoreCase));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassCrossfadeStreamInputConfiguration.MODE_ELEMENT
            ).ConnectValue(option => this.Mode = BassCrossfadeStreamInputConfiguration.GetMode(option));
            this.Configuration.GetElement<IntegerConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassCrossfadeStreamInputConfiguration.PERIOD_IN_ELEMENT
            ).ConnectValue(value => this.InPeriod = value);
            this.Configuration.GetElement<IntegerConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassCrossfadeStreamInputConfiguration.PERIOD_OUT_ELEMENT
            ).ConnectValue(value => this.OutPeriod = value);
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassCrossfadeStreamInputConfiguration.TYPE_IN_ELEMENT
            ).ConnectValue(option => this.InType = BassCrossfadeStreamInputConfiguration.GetType(option));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassCrossfadeStreamInputConfiguration.TYPE_OUT_ELEMENT
            ).ConnectValue(option => this.OutType = BassCrossfadeStreamInputConfiguration.GetType(option));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassCrossfadeStreamInputConfiguration.MIX_ELEMENT
            ).ConnectValue(value => this.Mix = value);
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
            e.Input = new BassCrossfadeStreamInput(this, e.Stream);
            e.Input.InitializeComponent(this.Core);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassCrossfadeStreamInputConfiguration.GetConfigurationSections();
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
            Logger.Write(this, LogLevel.Debug, "Releasing BASS CROSSFADE.");
            BassCrossfade.Free();
            if (this.BassStreamPipelineFactory != null)
            {
                this.BassStreamPipelineFactory.CreatingPipeline -= this.OnCreatingPipeline;
            }
        }

        ~BassCrossfadeStreamInputBehaviour()
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