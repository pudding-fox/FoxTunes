using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryBrowserBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        public ArtworkGridProvider ArtworkGridProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public DoubleConfigurationElement ScalingFactor { get; private set; }

        public IntegerConfigurationElement TileSize { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ArtworkGridProvider = ComponentRegistry.Instance.GetComponent<ArtworkGridProvider>();
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.ScalingFactor.ValueChanged += this.OnValueChanged;
            this.TileSize = this.Configuration.GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_TILE_SIZE
            );
            this.TileSize.ValueChanged += this.OnValueChanged;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.InitializeComponent(core);
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
#if NET40
            var task = TaskEx.Run(() => this.ArtworkGridProvider.Clear());
#else
            var task = Task.Run(() => this.ArtworkGridProvider.Clear());
#endif
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.HierarchiesUpdated:
                    this.ArtworkGridProvider.Clear();
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
            return LibraryBrowserBehaviourConfiguration.GetConfigurationSections();
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
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
        }

        ~LibraryBrowserBehaviour()
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
