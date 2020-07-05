using System.Windows;

#if NET40
using Microsoft.Windows.Shell;
#else
using System.Windows.Shell;
#endif

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ToolWindow.xaml
    /// </summary>
    public partial class ToolWindow : Window
    {
        public ToolWindow()
        {
            this.SetValue(WindowChrome.WindowChromeProperty, new WindowChrome()
            {
                CaptionHeight = 30,
                ResizeBorderThickness = new Thickness(5)
            });
            this.InitializeComponent();
        }

        public ToolWindowConfiguration Configuration
        {
            get
            {
                var viewModel = this.TryFindResource("ViewModel") as global::FoxTunes.ViewModel.ToolWindow;
                if (viewModel == null)
                {
                    return null;
                }
                return viewModel.Configuration;
            }
            set
            {
                var viewModel = this.TryFindResource("ViewModel") as global::FoxTunes.ViewModel.ToolWindow;
                if (viewModel == null)
                {
                    return;
                }
                viewModel.Configuration = value;
            }
        }
    }
}
