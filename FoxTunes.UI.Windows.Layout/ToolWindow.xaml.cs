using System.ComponentModel;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ToolWindow.xaml
    /// </summary>
    public partial class ToolWindow : WindowBase
    {
        public ToolWindow()
        {
            this.InitializeComponent();
        }

        public override string Id
        {
            get
            {
                var configuration = this.Configuration;
                if (configuration == null)
                {
                    return string.Empty;
                }
                return configuration.Title;
            }
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
                if (viewModel.Configuration == null)
                {
                    return;
                }
                this.UpdateBounds(viewModel.Bounds);
            }
        }

        protected virtual void UpdateBounds(Rect bounds)
        {
            if (bounds.IsEmpty)
            {
                return;
            }
            if (bounds.Left != 0 && bounds.Right != 0 && ScreenHelper.WindowBoundsVisible(bounds))
            {
                this.Left = bounds.Left;
                this.Top = bounds.Top;
            }
            if (bounds.Width > 0)
            {
                this.Width = bounds.Width;
            }
            if (bounds.Height > 0)
            {
                this.Height = bounds.Height;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var viewModel = this.TryFindResource("ViewModel") as global::FoxTunes.ViewModel.ToolWindow;
            if (viewModel != null)
            {
                viewModel.Bounds = this.RestoreBounds;
            }
            base.OnClosing(e);
        }
    }
}
