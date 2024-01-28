using System;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Artwork.xaml
    /// </summary>
    [UIComponent("66C8A9E7-0891-48DD-8086-E40F72D4D030", role: UIComponentRole.Info)]
    public partial class Artwork : SquareUIComponentBase
    {
        const int TIMEOUT = 100;

        public Artwork()
        {
            this.Debouncer = new Debouncer(TIMEOUT);
            this.InitializeComponent();
            this.OnFileNameChanged(this, EventArgs.Empty);
        }

        public Debouncer Debouncer { get; private set; }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.Artwork>("ViewModel");
            if (viewModel != null)
            {
                this.Debouncer.Exec(viewModel.Emit);
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
