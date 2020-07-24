using System;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Artwork.xaml
    /// </summary>
    [UIComponent("66C8A9E7-0891-48DD-8086-E40F72D4D030", "Artwork")]
    public partial class Artwork : SquareUIComponentBase
    {
        public Artwork()
        {
            this.InitializeComponent();
            this.OnFileNameChanged(this, EventArgs.Empty);
        }

        protected override void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            base.OnSizeChanged(sender, e);
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.Artwork>("ViewModel");
            if (viewModel != null)
            {
                this.Dispatch(viewModel.Refresh);
            }
        }

        protected virtual void OnFileNameChanged(object sender, EventArgs e)
        {
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.Artwork>("ViewModel");
            if (viewModel != null)
            {
                this.IsComponentEnabled = !string.IsNullOrEmpty(viewModel.FileName) && File.Exists(viewModel.FileName);
            }
        }
    }
}
