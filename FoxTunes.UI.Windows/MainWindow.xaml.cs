using System.ComponentModel;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        public MainWindow()
        {
            if (!global::FoxTunes.Properties.Settings.Default.MainWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.MainWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Top;
                }
                this.Width = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Width;
                this.Height = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Height;
            }
            this.InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.MainWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
