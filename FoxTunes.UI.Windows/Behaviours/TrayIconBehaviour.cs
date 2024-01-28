using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes
{
    public class TrayIconBehaviour : StandardBehaviour, IInvocableComponent, IDisposable
    {
        public const string QUIT = "ZZZZ";

        public TrayIconBehaviour()
        {
            Windows.ActiveWindowChanging += (sender, e) =>
            {
                if (this.Enabled)
                {
                    this.Disable();
                }
            };
            Windows.ActiveWindowChanged += (sender, e) =>
            {
                if (this.Enabled)
                {
                    this.Enable();
                }
            };
        }

        public IConfiguration Configuration { get; private set; }

        public bool _Enabled { get; private set; }

        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
                this.OnEnabledChanged();
            }
        }

        protected virtual void OnEnabledChanged()
        {
            if (this.Enabled)
            {
                this.Enable();
            }
            else
            {
                this.Disable();
            }
        }

        public bool MinimizeToTray { get; private set; }

        public bool CloseToTray { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            if (this.Configuration.GetSection(NotifyIconConfiguration.SECTION) != null)
            {
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    NotifyIconConfiguration.SECTION,
                    NotifyIconConfiguration.ENABLED_ELEMENT
                ).ConnectValue(value => this.Enabled = value);
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    NotifyIconConfiguration.SECTION,
                    NotifyIconConfiguration.MINIMIZE_TO_TRAY_ELEMENT
                ).ConnectValue(value => this.MinimizeToTray = value);
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    NotifyIconConfiguration.SECTION,
                    NotifyIconConfiguration.CLOSE_TO_TRAY_ELEMENT
                ).ConnectValue(value => this.CloseToTray = value);
            }
            base.InitializeComponent(core);
        }

        protected virtual void Enable()
        {
            if (Windows.ActiveWindow != null)
            {
                Windows.ActiveWindow.StateChanged += this.OnStateChanged;
                Windows.ActiveWindow.Closing += this.OnClosing;
            }
        }

        protected virtual void Disable()
        {
            if (Windows.ActiveWindow != null)
            {
                Windows.ActiveWindow.StateChanged -= this.OnStateChanged;
                Windows.ActiveWindow.Closing -= this.OnClosing;
            }
        }

        protected virtual void OnClose(object sender, EventArgs e)
        {
            this.Disable();
            this.Quit();
        }

        protected virtual void Quit()
        {
            Windows.Shutdown();
        }

        protected virtual void OnStateChanged(object sender, EventArgs e)
        {
            if (this.MinimizeToTray)
            {
                if (Windows.ActiveWindow != null)
                {
                    if (Windows.ActiveWindow.WindowState == WindowState.Minimized)
                    {
                        Windows.ActiveWindow.Hide();
                    }
                }
            }
        }

        protected virtual void OnClosing(object sender, CancelEventArgs e)
        {
            if (this.CloseToTray)
            {
                e.Cancel = true;
                Windows.ActiveWindow.Hide();
            }
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_NOTIFY_ICON, QUIT, "Quit");
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case QUIT:
                    return Windows.Invoke(() =>
                    {
                        this.Disable();
                        this.OnClose(this, EventArgs.Empty);
                    });
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
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
            this.Disable();
        }

        ~TrayIconBehaviour()
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
