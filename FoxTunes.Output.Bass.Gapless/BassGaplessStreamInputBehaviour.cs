using FoxTunes.Interfaces;
using ManagedBass.Gapless;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    [Component("9B40FE6A-89F1-4F97-888C-05D7B34EC42A", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassGaplessStreamInputBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
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

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.INPUT_ELEMENT
            ).ConnectValue(value => this.Enabled = string.Equals(value.Id, BassGaplessStreamInputConfiguration.INPUT_GAPLESS_OPTION, StringComparison.OrdinalIgnoreCase));
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            this.BassStreamPipelineFactory.CreatingPipeline += this.OnCreatingPipeline;
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            BassUtils.OK(BassGapless.Init());
            this.IsInitialized = true;
            Logger.Write(this, LogLevel.Debug, "BASS GAPLESS Initialized.");
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            Logger.Write(this, LogLevel.Debug, "Releasing BASS GAPLESS.");
            BassGapless.Free();
            this.IsInitialized = false;
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            e.Input = new BassGaplessStreamInput(this);
            e.Input.InitializeComponent(this.Core);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassGaplessStreamInputConfiguration.GetConfigurationSections();
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
                this.BassStreamPipelineFactory.CreatingPipeline -= this.OnCreatingPipeline;
            }
        }

        ~BassGaplessStreamInputBehaviour()
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

    public delegate void BassGaplessEventHandler(object sender, BassGaplessEventArgs e);
}