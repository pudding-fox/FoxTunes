using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class WindowsUserInterface : UserInterface
    {
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
