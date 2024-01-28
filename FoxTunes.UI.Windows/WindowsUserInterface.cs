using FoxTunes.Interfaces;
using FoxTunes.Theme;
using System;
using System.Windows;

namespace FoxTunes
{
    [Component("B889313D-4F21-4794-8D16-C2FAE6A7B305", ComponentSlots.UserInterface)]
    public class WindowsUserInterface : UserInterface
    {
        public static readonly Type[] References = new[]
        {
            typeof(global::System.Windows.Interactivity.Interaction)
        };

        public ICore Core { get; private set; }

        public IThemeLoader ThemeLoader { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.ThemeLoader = new ThemeLoader();
            this.ThemeLoader.InitializeComponent(core);
            base.InitializeComponent(core);
        }

        public override void Show()
        {
            var application = new Application();
            this.ThemeLoader.Apply(application);
            application.Run(new Main() { DataContext = this.Core });
        }
    }
}
