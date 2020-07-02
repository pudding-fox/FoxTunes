using System.Collections.Generic;
using System.Windows;
using System.Linq;

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

        public IEnumerable<global::FoxTunes.ViewModel.Tool> ViewModels
        {
            get
            {
                return new FrameworkElement[] { this, this.Tool }.Select(
                    element => element.FindResource<global::FoxTunes.ViewModel.Tool>("ViewModel")
                );
            }
        }

        public ToolWindowConfiguration Configuration
        {
            get
            {
                foreach (var viewModel in this.ViewModels)
                {
                    return viewModel.Configuration;
                }
                return default(ToolWindowConfiguration);
            }
            set
            {
                foreach (var viewModel in this.ViewModels)
                {
                    viewModel.Configuration = value;
                }
            }
        }
    }
}
