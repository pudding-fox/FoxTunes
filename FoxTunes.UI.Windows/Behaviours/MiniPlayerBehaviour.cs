using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace FoxTunes
{
    public class MiniPlayerBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent, IDisposable
    {
        public const string QUIT = "ZZZZ";

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

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

        public bool _Topmost { get; private set; }

        public bool Topmost
        {
            get
            {
                return this._Topmost;
            }
            set
            {
                this._Topmost = value;
                this.OnTopmostChanged();
            }
        }

        protected virtual void OnTopmostChanged()
        {
            if (this.Enabled)
            {
                if (Application.Current != null && Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Topmost = this.Topmost;
                }
            }
        }

        protected virtual void Enable()
        {
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                global::FoxTunes.Properties.Settings.Default.MainWindowBounds = Application.Current.MainWindow.RestoreBounds;
                global::FoxTunes.Properties.Settings.Default.Save();
                if (Application.Current.MainWindow.WindowState != WindowState.Normal)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                }
                Application.Current.MainWindow.WindowStyle = WindowStyle.None;
                Application.Current.MainWindow.ResizeMode = ResizeMode.NoResize;
                Application.Current.MainWindow.SizeToContent = SizeToContent.WidthAndHeight;
                Application.Current.MainWindow.Topmost = this.Topmost;
                Application.Current.MainWindow.MouseDown += this.OnMouseDown;
            }
        }

        protected virtual void Disable()
        {
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                Application.Current.MainWindow.ResizeMode = ResizeMode.CanResize;
                Application.Current.MainWindow.SizeToContent = SizeToContent.Manual;
                Application.Current.MainWindow.Left = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Left;
                Application.Current.MainWindow.Top = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Top;
                Application.Current.MainWindow.Width = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Width;
                Application.Current.MainWindow.Height = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Height;
                Application.Current.MainWindow.Topmost = false;
                Application.Current.MainWindow.MouseDown -= this.OnMouseDown;
            }
        }

        protected virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (Application.Current != null && Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.DragMove();
                }
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                MiniPlayerBehaviourConfiguration.MINI_PLAYER_SECTION,
                MiniPlayerBehaviourConfiguration.ENABLED_ELEMENT
            ).ConnectValue<bool>(value => this.Enabled = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                MiniPlayerBehaviourConfiguration.MINI_PLAYER_SECTION,
                MiniPlayerBehaviourConfiguration.TOPMOST_ELEMENT
            ).ConnectValue<bool>(value => this.Topmost = value);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_MINI_PLAYER, QUIT, "Quit");
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case QUIT:
                    return this.ForegroundTaskRunner.Run(() => this.Quit());
            }
            return Task.CompletedTask;
        }

        protected virtual void Quit()
        {
            //We have to close the main window as a low priority task otherwise the dispatcher 
            //running *this* task is shut down which causes an exception.
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        if (Application.Current != null && Application.Current.MainWindow != null)
                        {
                            Application.Current.MainWindow.Close();
                        }
                    })
                );
            }
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return MiniPlayerBehaviourConfiguration.GetConfigurationSections();
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
            if (this.Enabled)
            {
                this.Disable();
            }
        }

        ~MiniPlayerBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
