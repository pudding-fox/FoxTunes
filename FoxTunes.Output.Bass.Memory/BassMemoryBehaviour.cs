using FoxTunes.Interfaces;
using ManagedBass.Memory;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassMemoryBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        public ICore Core { get; private set; }

        public IBassOutput Output { get; private set; }

        public IBassStreamPipelineFactory BassStreamPipelineFactory { get; private set; }

        public IConfiguration Configuration { get; private set; }

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
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            this.BassStreamPipelineFactory.CreatingPipeline += this.OnCreatingPipeline;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassMemoryBehaviourConfiguration.ENABLED_ELEMENT
            ).ConnectValue(value => this.Enabled = value);
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            BassMemory.Init();
            Logger.Write(this, LogLevel.Debug, "BASS MEMORY Initialized.");
            this.IsInitialized = true;
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Releasing BASS MEMORY.");
            BassMemory.Free();
            this.IsInitialized = false;
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            if (!BassMemoryStreamComponent.ShouldCreate(this, e.Stream, e.Query))
            {
                return;
            }
            var component = new BassMemoryStreamComponent(this, e.Stream.Flags);
            component.InitializeComponent(this.Core);
            e.Components.Add(component);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassMemoryBehaviourConfiguration.GetConfigurationSections();
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

        ~BassMemoryBehaviour()
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
