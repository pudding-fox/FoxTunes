using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for PeakMeter.xaml
    /// </summary>
    [UIComponent("F8231616-9D5E-45C8-BD72-506FC5FC9C95", role: UIComponentRole.Visualization)]
    public partial class PeakMeter : UIComponentBase
    {
        public PeakMeter()
        {
            this.InitializeComponent();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var size = sizeInfo.NewSize;
            if (!double.IsNaN(size.Width) && !double.IsNaN(size.Height) && size.Width > 0 && size.Height > 0)
            {
                var ratio = size.Width / size.Height;
                var orientation = default(Orientation);
                if (ratio > 1)
                {
                    orientation = Orientation.Horizontal;
                }
                else
                {
                    orientation = Orientation.Vertical;
                }
                var viewModel = this.FindResource<global::FoxTunes.ViewModel.PeakMeter>("ViewModel");
                if (viewModel != null && viewModel.Orientation != orientation)
                {
                    viewModel.Orientation = orientation;
                }
            }
            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}