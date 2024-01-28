using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Windows;

namespace FoxTunes
{
    public class TrayIconBehaviour : StandardBehaviour, IDisposable
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
                this.NotifyIcon.Click += this.OnClick;
                this.NotifyIcon.Visible = true;
            }
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.StateChanged += this.OnStateChanged;
                Application.Current.MainWindow.Closing += this.OnClosing;
            }
        }

        protected virtual void OnClick(object sender, EventArgs e)
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

        protected virtual void OnStateChanged(object sender, EventArgs e)
        {
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                {
                    Application.Current.MainWindow.Hide();
                }
            }
        }

        protected virtual void OnClosing(object sender, CancelEventArgs e)
        {
            //TODO: Close to tray.
        }

        protected virtual void Disable()
        {
            if (this.NotifyIcon != null)
            {
                this.NotifyIcon.Visible = false;
                this.NotifyIcon.Click -= this.OnClick;
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
