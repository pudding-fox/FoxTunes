using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            this.InitializeComponent();
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //If settings is open and the user switches to another program, 
            //the popup will not work properly until the main window is "focused".
            var window = this.Parent.FindAncestor<Window>();
            if (window != null && !window.IsActive)
            {
                window.Activate();
            }
        }
    }
}
