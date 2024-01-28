using FoxTunes.Interfaces;
using FoxTunes.Theme;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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

        public Application Application { get; private set; }

        public ICore Core { get; private set; }

        public IThemeLoader ThemeLoader { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
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
                if (File.Exists(match.Value))
                {
                    this.OnOpen(match.Value);
                }
            }
            //TODO: Respect IForegroundTaskRunner component.
            this.Application.Dispatcher.Invoke(() => this.Application.MainWindow.Activate());
        }

        protected virtual void OnOpen(string fileName)
        {
            if (!this.Core.Components.Output.IsSupported(fileName))
            {
                return;
            }
            this.Core.Managers.Playlist.Add(new[] { fileName }, false).ContinueWith(result => this.Core.Managers.Playlist.Play(fileName));
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
