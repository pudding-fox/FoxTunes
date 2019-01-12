using System;
using System.ComponentModel;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            if (!global::FoxTunes.Properties.Settings.Default.MainWindowBounds.IsEmpty())
            {
                this.Left = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Left;
                this.Top = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Top;
                this.Width = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Width;
                this.Height = global::FoxTunes.Properties.Settings.Default.MainWindowBounds.Height;
            }
            base.OnInitialized(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var miniPlayerBehaviour = ComponentRegistry.Instance.GetComponent<MiniPlayerBehaviour>();
            if (miniPlayerBehaviour == null || !miniPlayerBehaviour.Enabled)
            {
                global::FoxTunes.Properties.Settings.Default.MainWindowBounds = this.RestoreBounds;
            }
            else
            {
                global::FoxTunes.Properties.Settings.Default.MiniWindowBounds = this.RestoreBounds;
            }
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }

    public static partial class Extensions
    {
        public static bool IsEmpty(this Rect rect)
        {
            return
                (rect.Left == 0 || double.IsInfinity(rect.Left)) &&
                (rect.Top == 0 || double.IsInfinity(rect.Top)) &&
                (rect.Width == 0 || double.IsInfinity(rect.Width)) &&
                (rect.Height == 0 || double.IsInfinity(rect.Height));
        }
    }
}
