using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Playback.xaml
    /// </summary>
    public partial class Playback : UserControl
    {
        public Playback()
        {
            this.InitializeComponent();
        }

        protected virtual void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.Playback>("ViewModel");
            if (viewModel == null)
            {
                return;
            }
            var task = viewModel.Refresh();
        }
    }
}
