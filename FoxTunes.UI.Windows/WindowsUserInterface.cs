using FoxTunes.Interfaces;
using FoxTunes.Theme;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using System.Linq;
using System.Threading.Tasks;

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
            this.Queue = new PendingQueue<string>(TimeSpan.FromMilliseconds(100));
            this.Queue.Complete += async (sender, e) =>
            {
                using (e.Defer())
                {
                    await this.OnOpen(e.Sequence);
                }
            };
        }

        public PendingQueue<string> Queue { get; private set; }

        public Application Application { get; private set; }

        public ICore Core { get; private set; }

        public IOutput Output { get; private set; }

        public IPlaylistManager Playlist { get; private set; }

        public IThemeLoader ThemeLoader { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = core.Components.Output;
            this.Playlist = core.Managers.Playlist;
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<IThemeLoader>();
            base.InitializeComponent(core);
        }

        public override void Show()
        {
            this.Application = new Application();
            this.Application.DispatcherUnhandledException += this.OnApplicationDispatcherUnhandledException;
            this.ThemeLoader.Application = this.Application;
            this.Application.Run(new MainWindow() { DataContext = this.Core });
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

        protected virtual Task OnOpen(IEnumerable<string> paths)
        {
            var index = this.Playlist.GetInsertIndex();
            return this.Playlist.Add(paths, false).ContinueWith(task => this.Playlist.Play(index));
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
