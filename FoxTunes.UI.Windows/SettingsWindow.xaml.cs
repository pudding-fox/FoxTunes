using System.ComponentModel;
using System.Windows;

#if NET40
using Microsoft.Windows.Shell;
#else
using System.Windows.Shell;
#endif

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            this.SetValue(WindowChrome.WindowChromeProperty, new WindowChrome()
            {
                CaptionHeight = 30,
                ResizeBorderThickness = new Thickness(5)
            });
            if (!global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds.IsEmpty())
            {
                this.Left = global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds.Left;
                this.Top = global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds.Top;
                this.Width = global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds.Width;
                this.Height = global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds.Height;
            }
            else
            {
                this.Width = 800;
                this.Height = 600;
            }
            this.InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.SettingsWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
