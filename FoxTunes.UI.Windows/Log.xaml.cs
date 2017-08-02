using System.ComponentModel;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Log.xaml
    /// </summary>
    public partial class Log : Window
    {
        public Log()
        {
            this.PreventClose = true;
            this.InitializeComponent();
        }

        public bool PreventClose { get; set; }

        protected virtual void OnClosing(object sender, CancelEventArgs e)
        {
            (this.FindResource("ViewModel") as ViewModel.Log).LogVisible = false;
            e.Cancel = this.PreventClose;
        }
    }
}
