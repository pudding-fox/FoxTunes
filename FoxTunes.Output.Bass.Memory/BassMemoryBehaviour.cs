using FoxTunes.Interfaces;
using ManagedBass.Memory;
using System;
using System.Collections.Generic;
using System.IO;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassMemoryBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        public static string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(BassMemoryBehaviour).Assembly.Location);
            }
        }

        public BassMemoryBehaviour()
        {
            BassPluginLoader.AddPath(Path.Combine(Location, "Addon"));
            BassPluginLoader.AddPath(Path.Combine(Loader.FolderName, "bass_memory.dll"));
        }

        public ICore Core { get; private set; }

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
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            this.BassStreamPipelineFactory.CreatingPipeline += this.OnCreatingPipeline;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassMemoryBehaviourConfiguration.ENABLED_ELEMENT
            ).ConnectValue(value => this.Enabled = value);
            base.InitializeComponent(core);
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
            var component = new BassMemoryStreamComponent(this, e.Pipeline, e.Stream.Flags);
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
