using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for MiniWindow.xaml
    /// </summary>
    public partial class MiniWindow : Window
    {
        public MiniWindow()
        {
            if (!global::FoxTunes.Properties.Settings.Default.MiniWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.MiniWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.MiniWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.MiniWindowBounds.Top;
                }
            }
            this.InitializeComponent();
        }

        protected virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.MiniWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
