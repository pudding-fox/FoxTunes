using FoxTunes.Interfaces;
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
            ComponentRegistry.Instance.GetComponent<IConfiguration>().GetElement<BooleanConfigurationElement>(
                WindowsUserInterfaceConfiguration.APPEARANCE_SECTION,
                WindowsUserInterfaceConfiguration.SHOW_TRAY_ICON_ELEMENT
            ).ConnectValue<bool>(value => ComponentRegistry.Instance.GetComponent<TrayIconBehaviour>().Enabled = value);
            base.OnInitialized(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.MainWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }

    public static partial class Extensions
    {
        public static bool IsEmpty(this Rect rect)
        {
            return rect.Left == 0 && rect.Top == 0 && rect.Width == 0 && rect.Height == 0;
        }
    }
}
