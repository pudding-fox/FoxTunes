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

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

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
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<BooleanConfigurationElement>(
                NotifyIconConfiguration.NOTIFY_ICON_SECTION,
                NotifyIconConfiguration.ENABLED_ELEMENT
            ).ConnectValue<bool>(value => this.Enabled = value);
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<BooleanConfigurationElement>(
                NotifyIconConfiguration.NOTIFY_ICON_SECTION,
                NotifyIconConfiguration.MINIMIZE_TO_TRAY_ELEMENT
            ).ConnectValue<bool>(value => this.MinimizeToTray = value);
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<BooleanConfigurationElement>(
                NotifyIconConfiguration.NOTIFY_ICON_SECTION,
                NotifyIconConfiguration.CLOSE_TO_TRAY_ELEMENT
            ).ConnectValue<bool>(value => this.CloseToTray = value);
            base.InitializeComponent(core);
        }

        protected virtual void Enable()
        {
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.StateChanged += this.OnStateChanged;
                Application.Current.MainWindow.Closing += this.OnClosing;
            }
        }

        protected virtual void OnClose(object sender, EventArgs e)
        {
            this.Disable();
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Close();
            }
        }

        protected virtual void OnStateChanged(object sender, EventArgs e)
        {
            if (this.MinimizeToTray)
            {
                if (Application.Current != null && Application.Current.MainWindow != null)
                {
                    if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                    {
                        Application.Current.MainWindow.Hide();
                    }
                }
            }
        }

        protected virtual void OnClosing(object sender, CancelEventArgs e)
        {
            if (this.CloseToTray)
            {
                e.Cancel = true;
                Application.Current.MainWindow.Hide();
            }
        }

        protected virtual void Disable()
        {
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.StateChanged -= this.OnStateChanged;
                Application.Current.MainWindow.Closing -= this.OnClosing;
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
                    return this.ForegroundTaskRunner.Run(() =>
                    {
                        this.Disable();
                        this.OnClose(this, EventArgs.Empty);
                    });
            }
            return Task.CompletedTask;
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
            this.Dispose(true);
        }
    }
}
