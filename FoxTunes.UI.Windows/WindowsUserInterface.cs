using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
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
