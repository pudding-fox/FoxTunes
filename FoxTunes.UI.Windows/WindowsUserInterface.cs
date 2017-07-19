using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    [Component("B889313D-4F21-4794-8D16-C2FAE6A7B305", ComponentSlots.UserInterface)]
    public class WindowsUserInterface : UserInterface
    {
        public static readonly Type[] References = new[]
        {
            typeof(global::System.Windows.Interactivity.Interaction)
        };

        private Main Main { get; set; }

        public override void InitializeComponent(ICore core)
        {
            this.Main = new Main();
            this.Main.DataContext = core;
            base.InitializeComponent(core);
        }

        public override void Show()
        {
            this.Main.ShowDialog();
        }
    }
}
