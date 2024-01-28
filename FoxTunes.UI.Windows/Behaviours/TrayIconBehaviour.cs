using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class TrayIconBehaviour : StandardBehaviour, IInvocableComponent, IDisposable
    {
        public const string QUIT = "ZZZZ";

        private static readonly Lazy<Icon> _Icon = new Lazy<Icon>(() =>
        {
            using (var stream = typeof(Mini).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Images.Fox.ico"))
            {
                if (stream == null)
                {
                    return null;
                }
                return new Icon(stream);
            }
        });

        public static Icon Icon
        {
            get
            {
                return _Icon.Value;
            }
        }

        private static readonly Lazy<Menu> _Menu = new Lazy<Menu>(() =>
        {
            return new Menu()
            {
                Category = InvocationComponent.CATEGORY_NOTIFY_ICON,
                Placement = PlacementMode.AbsolutePoint
            };
        });

        public static Menu Menu
        {
            get
            {
                return _Menu.Value;
            }
        }

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

        public INotifyIcon NotifyIcon { get; private set; }

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
            this.NotifyIcon = ComponentRegistry.Instance.GetComponent<INotifyIcon>();
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
            this.NotifyIcon.Icon = Icon.Handle;
            this.NotifyIcon.Show();
            if (this.NotifyIcon.MessageSink != null)
            {
                this.NotifyIcon.MessageSink.MouseLeftButtonUp += this.OnMouseLeftButtonUp;
                this.NotifyIcon.MessageSink.MouseRightButtonUp += this.OnMouseRightButtonUp;
            }
            if (Windows.ActiveWindow != null)
            {
                Windows.ActiveWindow.StateChanged += this.OnStateChanged;
                Windows.ActiveWindow.Closing += this.OnClosing;
            }
        }

        protected virtual void Disable()
        {
            if (this.NotifyIcon != null)
            {
                this.NotifyIcon.Hide();
                if (this.NotifyIcon.MessageSink != null)
                {
                    this.NotifyIcon.MessageSink.MouseLeftButtonUp -= this.OnMouseLeftButtonUp;
                    this.NotifyIcon.MessageSink.MouseRightButtonUp -= this.OnMouseRightButtonUp;
                }
            }
            if (Windows.ActiveWindow != null)
            {
                Windows.ActiveWindow.StateChanged -= this.OnStateChanged;
                Windows.ActiveWindow.Closing -= this.OnClosing;
            }
        }

        protected virtual void OnMouseLeftButtonUp(object sender, EventArgs e)
        {
            var task = Windows.Invoke(() =>
            {
                Windows.ActiveWindow.Show();
                if (Windows.ActiveWindow.WindowState == WindowState.Minimized)
                {
                    Windows.ActiveWindow.WindowState = WindowState.Normal;
                }
                Windows.ActiveWindow.BringToFront();
            });
        }

        protected virtual void OnMouseRightButtonUp(object sender, EventArgs e)
        {
            var task = Windows.Invoke(() =>
            {
                var x = default(int);
                var y = default(int);
                MouseHelper.GetPosition(out x, out y);
                DpiHelper.TransformPosition(ref x, ref y);

                Menu.HorizontalOffset = x;
                Menu.VerticalOffset = y;
                Menu.IsOpen = true;

                var source = PresentationSource.FromVisual(Menu) as HwndSource;
                if (source != null && source.Handle != IntPtr.Zero)
                {
                    SetForegroundWindow(source.Handle);
                }
            });
        }

        protected virtual void OnClose(object sender, EventArgs e)
        {
            this.Disable();
            var task = this.Quit();
        }

        protected virtual Task Quit()
        {
            return Windows.Shutdown();
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

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
