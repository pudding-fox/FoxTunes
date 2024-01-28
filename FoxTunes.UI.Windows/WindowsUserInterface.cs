using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FoxTunes
{
    [Component("B889313D-4F21-4794-8D16-C2FAE6A7B305", ComponentSlots.UserInterface, priority: ComponentAttribute.PRIORITY_LOW)]
    public class WindowsUserInterface : UserInterface, IConfigurableComponent, IDisposable
    {
        public static readonly Type[] References = new[]
        {
            typeof(global::System.Windows.Interactivity.Interaction)
        };

        public WindowsUserInterface()
        {
            this.Application = new Application();
            this.Application.DispatcherUnhandledException += this.OnApplicationDispatcherUnhandledException;
            this.Queue = new PendingQueue<KeyValuePair<IFileActionHandler, string>>(TimeSpan.FromSeconds(1));
            this.Queue.Complete += this.OnComplete;
            Windows.MainWindowCreated += this.OnWindowCreated;
            Windows.MiniWindowCreated += this.OnWindowCreated;
        }

        public CommandLineParser.OpenMode OpenMode { get; private set; }

        private Application _Application { get; set; }

        public Application Application
        {
            get
            {
                return this._Application;
            }
            private set
            {
                this._Application = value;
                this.OnApplicationChanged();
            }
        }

        protected virtual void OnApplicationChanged()
        {
            if (this.ApplicationChanged != null)
            {
                this.ApplicationChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Application");
        }

        public event EventHandler ApplicationChanged;

        public PendingQueue<KeyValuePair<IFileActionHandler, string>> Queue { get; private set; }

        public ICore Core { get; private set; }

        public IOutput Output { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IEnumerable<IFileActionHandler> FileActionHandlers { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output;
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.PlaylistManager = core.Managers.Playlist;
            this.FileActionHandlers = ComponentRegistry.Instance.GetComponents<IFileActionHandler>();
            base.InitializeComponent(core);
        }

        public override void Show()
        {
            if (Windows.IsMiniWindowCreated)
            {
                Windows.MiniWindow.DataContext = this.Core;
                this.Application.Run(Windows.MiniWindow);
            }
            else
            {
                Windows.MainWindow.DataContext = this.Core;
                this.Application.Run(Windows.MainWindow);
            }
        }

        public override void Activate()
        {
            Windows.Invoke(() =>
            {
                if (Windows.ActiveWindow != null)
                {
                    if (Windows.ActiveWindow.WindowState == WindowState.Minimized)
                    {
                        Windows.ActiveWindow.WindowState = WindowState.Normal;
                    }
                    Windows.ActiveWindow.Activate();
                }
            });
        }

        public override void Run(string message)
        {
            var mode = default(CommandLineParser.OpenMode);
            var paths = default(IEnumerable<string>);
            if (!CommandLineParser.TryParse(message, out paths, out mode))
            {
                return;
            }
            this.OpenMode = mode;
            foreach (var path in paths)
            {
                foreach (var handler in this.FileActionHandlers)
                {
                    if (handler.CanHandle(path))
                    {
                        this.Queue.Enqueue(new KeyValuePair<IFileActionHandler, string>(handler, path));
                        break;
                    }
                }
            }
        }

        public override void Warn(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public override void Fatal(Exception exception)
        {
            MessageBox.Show(exception.Message, "Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public override void Restart()
        {
            MessageBox.Show("Restart is required.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        protected virtual void OnComplete(object sender, PendingQueueEventArgs<KeyValuePair<IFileActionHandler, string>> e)
        {
            var task = this.OnOpen(e.Sequence);
        }

        protected virtual async Task OnOpen(IEnumerable<KeyValuePair<IFileActionHandler, string>> sequence)
        {
            try
            {
                var index = await this.PlaylistBrowser.GetInsertIndex().ConfigureAwait(false);
                var handlers = new Dictionary<IFileActionHandler, IList<string>>();
                foreach (var element in sequence)
                {
                    handlers.GetOrAdd(element.Key, key => new List<string>()).Add(element.Value);
                }
                foreach (var pair in handlers)
                {
                    await pair.Key.Handle(pair.Value);
                }
                if (this.OpenMode == CommandLineParser.OpenMode.Play)
                {
                    await this.PlaylistManager.Play(index).ConfigureAwait(false);
                }
            }
            finally
            {
                this.OpenMode = CommandLineParser.OpenMode.Default;
            }
        }

        protected virtual void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Write(this, LogLevel.Fatal, e.Exception.Message, e);
            //Don't crash out.
            e.Handled = true;
        }

        protected virtual void OnWindowCreated(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window == null)
            {
                return;
            }
            this.OnWindowCreated(window.GetHandle());
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WindowsUserInterfaceConfiguration.GetConfigurationSections();
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
            if (this.Queue != null)
            {
                this.Queue.Complete -= this.OnComplete;
            }
            Windows.MainWindowCreated -= this.OnWindowCreated;
            Windows.MiniWindowCreated -= this.OnWindowCreated;
        }

        ~WindowsUserInterface()
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
