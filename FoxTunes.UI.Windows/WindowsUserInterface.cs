using FoxTunes.Interfaces;
using FoxTunes.Theme;
using System;
using System.Collections.Generic;
using System.Windows;

namespace FoxTunes
{
    [Component("B889313D-4F21-4794-8D16-C2FAE6A7B305", ComponentSlots.UserInterface)]
    public class WindowsUserInterface : UserInterface, IConfigurableComponent
    {
        public static readonly Type[] References = new[]
        {
            typeof(global::System.Windows.Interactivity.Interaction)
        };

        public Application Application { get; private set; }

        public ICore Core { get; private set; }

        public ILogEmitter LogEmitter { get; private set; }

        public IThemeLoader ThemeLoader { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.LogEmitter = core.Components.LogEmitter;
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<IThemeLoader>();
            base.InitializeComponent(core);
        }

        public override void Show()
        {
            this.Application = new Application();
            this.Application.Exit += this.OnApplicationExit;
            this.ThemeLoader.Application = this.Application;
            this.Application.Run(new Main() { DataContext = this.Core });
        }

        protected virtual void OnApplicationExit(object sender, ExitEventArgs e)
        {
            this.LogEmitter.Enabled = false;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WindowsUserInterfaceConfiguration.GetConfigurationSections();
        }
    }
}
