using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Titlebar.xaml
    /// </summary>
    public partial class Titlebar : UserControl
    {
        public Titlebar()
        {
            this.InitializeComponent();
        }

        protected virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (Application.Current != null && Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.DragMove();
                }
            }
        }
    }
}
