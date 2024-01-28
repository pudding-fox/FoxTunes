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
    public class WindowsUserInterface : UserInterface, IConfigurableComponent
    {
        public static readonly Type[] References = new[]
        {
            typeof(global::System.Windows.Interactivity.Interaction)
        };

        public WindowsUserInterface()
        {
            this.Application = new Application();
            this.Application.DispatcherUnhandledException += this.OnApplicationDispatcherUnhandledException;
            this.Window = new MainWindow();
            this.Queue = new PendingQueue<string>(TimeSpan.FromMilliseconds(100));
            this.Queue.Complete += async (sender, e) =>
            {
                using (e.Defer())
                {
                    await this.OnOpen(e.Sequence);
                }
            };
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

        public event EventHandler ApplicationChanged = delegate { };

        private Window _Window { get; set; }

        public Window Window
        {
            get
            {
                return this._Window;
            }
            private set
            {
                this._Window = value;
                this.OnWindowChanged();
            }
        }

        protected virtual void OnWindowChanged()
        {
            if (this.WindowChanged != null)
            {
                this.WindowChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Window");
        }

        public event EventHandler WindowChanged = delegate { };

        public PendingQueue<string> Queue { get; private set; }

        public ICore Core { get; private set; }

        public IOutput Output { get; private set; }

        public IPlaylistManager Playlist { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output;
            this.Playlist = core.Managers.Playlist;
            this.Window.DataContext = core;
            base.InitializeComponent(core);
        }

        public override void Show()
        {
            this.Application.Run(this.Window);
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
                if ((File.Exists(path) && this.Output.IsSupported(path)) || Directory.Exists(path))
                {
                    this.Queue.Enqueue(path);
                }
            }
        }

        public override void Fatal(Exception exception)
        {
            MessageBox.Show(exception.Message, "Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected virtual async Task OnOpen(IEnumerable<string> paths)
        {
            var index = await this.Playlist.GetInsertIndex();
            await this.Playlist.Add(paths, false);
            await this.Playlist.Play(index);
        }

        protected virtual void OnApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Write(this, LogLevel.Fatal, e.Exception.Message, e);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WindowsUserInterfaceConfiguration.GetConfigurationSections();
        }
    }
}
