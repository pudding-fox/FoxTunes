using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Artwork.xaml
    /// </summary>
    [UIComponent("66C8A9E7-0891-48DD-8086-E40F72D4D030", UIComponentSlots.NONE, "Artwork")]
    [UIComponentDependency(MetaDataBehaviourConfiguration.SECTION, MetaDataBehaviourConfiguration.ENABLE_ELEMENT)]
    public partial class Artwork : SquareUIComponentBase
    {
        public static readonly IArtworkProvider ArtworkProvider = ComponentRegistry.Instance.GetComponent<IArtworkProvider>();

        public static readonly IPlaybackManager PlaybackManager = ComponentRegistry.Instance.GetComponent<IPlaybackManager>();

        public static readonly ThemeLoader ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();

        public static readonly ImageLoader ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();

        public static readonly IConfiguration Configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();

        public Artwork()
        {
            this.InitializeComponent();
            if (PlaybackManager != null)
            {
                PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            }
            if (ThemeLoader != null)
            {
                ThemeLoader.ThemeChanged += this.OnThemeChanged;
            }
            if (Configuration != null)
            {
                Configuration.GetElement<DoubleConfigurationElement>(
                    WindowsUserInterfaceConfiguration.SECTION,
                    WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
                ).ConnectValue(value => this.ScalingFactor = value);
            }
        }

        private double _ScalingFactor { get; set; }

        public double ScalingFactor
        {
            get
            {
                return this._ScalingFactor;
            }
            set
            {
                this._ScalingFactor = value;
                this.OnScalingFactorChanged();
            }
        }

        protected virtual void OnScalingFactorChanged()
        {
            if (this.IsInitialized)
            {
                this.Dispatch(this.Refresh);
            }
            if (this.ScalingFactorChanged != null)
            {
                this.ScalingFactorChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ScalingFactor");
        }

        public event EventHandler ScalingFactorChanged;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Dispatch(this.Refresh);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Background = null;
        }

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            this.Dispatch(this.Refresh);
        }

        protected virtual void OnThemeChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.Refresh);
        }

        protected override void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            base.OnSizeChanged(sender, e);
            this.Dispatch(this.Refresh);
        }

        public async Task Refresh()
        {
            if (double.IsNaN(this.ActualWidth) || double.IsNaN(this.ActualHeight) || this.ActualWidth == 0 || this.ActualHeight == 0)
            {
                //No size is available.
                return;
            }
            var fileName = default(string);
            var outputStream = PlaybackManager.CurrentStream;
            if (outputStream != null)
            {
                fileName = ArtworkProvider.Find(
                    outputStream.PlaylistItem,
                    ArtworkType.FrontCover
                );
            }
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                var source = ImageLoader.Load(
                    ThemeLoader.Theme.Id,
                    () => ThemeLoader.Theme.ArtworkPlaceholder,
                    Convert.ToInt32(this.ActualWidth * this.ScalingFactor),
                    Convert.ToInt32(this.ActualHeight * this.ScalingFactor),
                    true
                );
                var brush = new ImageBrush(source)
                {
                    Stretch = Stretch.Uniform
                };
                brush.Freeze();
                await Windows.Invoke(() =>
                {
                    this.Background = brush;
                    this.IsComponentEnabled = false;
                }).ConfigureAwait(false);
            }
            else
            {
                var source = ImageLoader.Load(
                    fileName,
                    Convert.ToInt32(this.ActualWidth * this.ScalingFactor),
                    Convert.ToInt32(this.ActualHeight * this.ScalingFactor),
                    true
                );
                var brush = new ImageBrush(source)
                {
                    Stretch = Stretch.Uniform
                };
                brush.Freeze();
                await Windows.Invoke(() =>
                {
                    this.Background = brush;
                    this.IsComponentEnabled = true;
                }).ConfigureAwait(false);
            }
        }
    }
}
