using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ImageBehaviour : StandardBehaviour, IInvocableComponent, IDisposable
    {
        const string REFRESH_IMAGES = "ZAAA";

        public ThemeLoader ThemeLoader { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            this.ThemeLoader.ThemeChanged += this.OnThemeChanged;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            ).ConnectValue(value =>
            {
                if (this.IsInitialized)
                {
                    this.Dispatch(this.RefreshImages);
                }
            });
            base.InitializeComponent(core);
        }

        protected virtual void OnThemeChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.RefreshImages);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, REFRESH_IMAGES, "Refresh Images", path: "Library");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case REFRESH_IMAGES:
                    return this.RefreshImages();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        private Task RefreshImages()
        {
            return this.SignalEmitter.Send(new Signal(this, CommonSignals.ImagesUpdated));
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
            if (this.ThemeLoader != null)
            {
                this.ThemeLoader.ThemeChanged += this.OnThemeChanged;
            }
        }

        ~ImageBehaviour()
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
