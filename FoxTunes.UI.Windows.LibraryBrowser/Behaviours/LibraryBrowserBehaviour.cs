using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LibraryBrowserBehaviour : StandardBehaviour, IConfigurableComponent
    {
        const int TIMEOUT = 1000;

        public LibraryBrowserBehaviour()
        {
            this.Debouncer = new Debouncer(TIMEOUT);
        }

        public Debouncer Debouncer { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement ImageMode { get; private set; }

        public IntegerConfigurationElement TileSize { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Configuration = core.Components.Configuration;
            this.ImageMode = this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_TILE_IMAGE
            );
            this.TileSize = this.Configuration.GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_TILE_SIZE
            );
            this.ImageMode.ValueChanged += this.OnValueChanged;
            this.TileSize.ValueChanged += this.OnValueChanged;
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.HierarchiesUpdated:
                    this.Debouncer.Exec(() => this.Dispatch(this.RefreshImages));
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.Debouncer.Exec(() => this.Dispatch(this.RefreshImages));
        }

        private Task RefreshImages()
        {
            return this.SignalEmitter.Send(new Signal(this, CommonSignals.ImagesUpdated));
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
            if (this.ImageMode != null)
            {
                this.ImageMode.ValueChanged += this.OnValueChanged;
            }
            if (this.TileSize != null)
            {
                this.TileSize.ValueChanged += this.OnValueChanged;
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
