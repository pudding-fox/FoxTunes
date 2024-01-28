using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace FoxTunes
{
    public class TrayIconBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
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
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<BooleanConfigurationElement>(
                WindowsUserInterfaceConfiguration.APPEARANCE_SECTION,
                TrayIconBehaviourConfiguration.TRAY_ICON_ELEMENT
            ).ConnectValue<bool>(value => this.Enabled = value);
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<BooleanConfigurationElement>(
                WindowsUserInterfaceConfiguration.APPEARANCE_SECTION,
                TrayIconBehaviourConfiguration.MINIMIZE_TO_TRAY_ELEMENT
            ).ConnectValue<bool>(value => this.MinimizeToTray = value);
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<BooleanConfigurationElement>(
                WindowsUserInterfaceConfiguration.APPEARANCE_SECTION,
                TrayIconBehaviourConfiguration.CLOSE_TO_TRAY_ELEMENT
            ).ConnectValue<bool>(value => this.CloseToTray = value);
            base.InitializeComponent(core);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return TrayIconBehaviourConfiguration.GetConfigurationSections();
        }

        //TODO: System.Windows.Forms
        public global::System.Windows.Forms.NotifyIcon NotifyIcon { get; private set; }

        protected virtual void Enable()
        {
            if (this.NotifyIcon == null)
            {
                //TODO: System.Windows.Forms
                this.NotifyIcon = new global::System.Windows.Forms.NotifyIcon();
                //TODO: System.Drawing
                this.NotifyIcon.Icon = new global::System.Drawing.Icon(
                    Application.GetResourceStream(new Uri("pack://application:,,,/FoxTunes.UI.Windows;component/Images/Fox.ico")).Stream
                );
                //TODO: System.Windows.Forms
                this.NotifyIcon.ContextMenu = new global::System.Windows.Forms.ContextMenu(new[]
                {
                    //TODO: System.Windows.Forms
                    new global::System.Windows.Forms.MenuItem("Quit", this.OnClose)
                });
                this.NotifyIcon.Click += this.OnOpen;
                this.NotifyIcon.Visible = true;
            }
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.StateChanged += this.OnStateChanged;
                Application.Current.MainWindow.Closing += this.OnClosing;
            }
        }

        protected virtual void OnOpen(object sender, EventArgs e)
        {
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Show();
                if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                }
                Application.Current.MainWindow.Activate();
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
            if (this.NotifyIcon != null)
            {
                this.NotifyIcon.Dispose();
                this.NotifyIcon = null;
            }
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.StateChanged -= this.OnStateChanged;
                Application.Current.MainWindow.Closing -= this.OnClosing;
            }
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
