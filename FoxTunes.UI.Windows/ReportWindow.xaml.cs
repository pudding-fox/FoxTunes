using System.Collections.Generic;
using System.Windows;
using FoxTunes.Interfaces;
using System.Linq;

#if NET40
using Microsoft.Windows.Shell;
#else
using System.Windows.Shell;
#endif

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        public ReportWindow()
        {
            this.SetValue(WindowChrome.WindowChromeProperty, new WindowChrome()
            {
                CaptionHeight = 30,
                ResizeBorderThickness = new Thickness(5)
            });
            this.InitializeComponent();
        }

        public IEnumerable<global::FoxTunes.ViewModel.Report> ViewModels
        {
            get
            {
                return new FrameworkElement[] { this, this.Report }.Select(
                    element => element.FindResource<global::FoxTunes.ViewModel.Report>("ViewModel")
                );
            }
        }

        public IReport Source
        {
            get
            {
                foreach (var viewModel in this.ViewModels)
                {
                    return viewModel.Source;
                }
                return default(IReport);
            }
            set
            {
                foreach (var viewModel in this.ViewModels)
                {
                    viewModel.Source = value;
                }
            }
        }

        protected virtual void OnClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
