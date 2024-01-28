using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
            this.Queue = new PendingQueue<string>(TimeSpan.FromMilliseconds(100));
            this.Queue.Complete += this.OnComplete;
            Windows.MainWindowCreated += this.OnWindowCreated;
            Windows.MiniWindowCreated += this.OnWindowCreated;
        }

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

        public PendingQueue<string> Queue { get; private set; }

        public ICore Core { get; private set; }

        public IOutput Output { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output;
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.PlaylistManager = core.Managers.Playlist;
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

        public override void Run(string message)
        {
            var regex = new Regex(@"((?:[a-zA-Z]\:(\\|\/)|file\:\/\/|\\\\|\.(\/|\\))([^\\\/\:\*\?\<\>\""\|]+(\\|\/){0,1})+)");
            var matches = regex.Matches(message);
            for (var a = 0; a < matches.Count; a++)
            {
                var match = matches[a];
                if (!match.Success)
                {
                    continue;
                }
                var path = match.Value;
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }
                path = path.Trim();
                if ((File.Exists(path) && this.Output.IsSupported(path)) || Directory.Exists(path))
                {
                    var task = this.Queue.Enqueue(path);
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

        protected virtual async void OnComplete(object sender, PendingQueueEventArgs<string> e)
        {
            using (e.Defer())
            {
                await this.OnOpen(e.Sequence).ConfigureAwait(false);
            }
        }

        protected virtual async Task OnOpen(IEnumerable<string> paths)
        {
            var index = await this.PlaylistBrowser.GetInsertIndex().ConfigureAwait(false);
            await this.PlaylistManager.Add(paths, false).ConfigureAwait(false);
            await this.PlaylistManager.Play(index).ConfigureAwait(false);
        }

        protected virtual void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Write(this, LogLevel.Fatal, e.Exception.Message, e);
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
