using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Artwork.xaml
    /// </summary>
    [UIComponent("66C8A9E7-0891-48DD-8086-E40F72D4D030", UIComponentSlots.BOTTOM_LEFT, "Artwork")]
    public partial class Artwork : UIComponentBase
    {
        public static readonly IArtworkProvider ArtworkProvider = ComponentRegistry.Instance.GetComponent<IArtworkProvider>();

        public static readonly IPlaybackManager PlaybackManager = ComponentRegistry.Instance.GetComponent<IPlaybackManager>();

        public static readonly ThemeLoader ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();

        private static Lazy<Size> PixelSize { get; set; }

        public static readonly DependencyProperty ShowPlaceholderProperty = DependencyProperty.Register(
            "ShowPlaceholder",
            typeof(bool),
            typeof(Artwork),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnShowPlaceholderChanged))
        );

        public static bool GetShowPlaceholder(Artwork source)
        {
            return (bool)source.GetValue(ShowPlaceholderProperty);
        }

        public static void SetShowPlaceholder(Artwork source, bool value)
        {
            source.SetValue(ShowPlaceholderProperty, value);
        }

        private static void OnShowPlaceholderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var artwork = sender as Artwork;
            if (artwork == null)
            {
                return;
            }
            artwork.OnShowPlaceholderChanged();
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource",
            typeof(ImageSource),
            typeof(Artwork),
            new FrameworkPropertyMetadata(default(ImageSource), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnImageSourceChanged))
        );

        public static ImageSource GetImageSource(Artwork source)
        {
            return (ImageSource)source.GetValue(ImageSourceProperty);
        }

        public static void SetImageSource(Artwork source, ImageSource value)
        {
            source.SetValue(ImageSourceProperty, value);
        }

        private static void OnImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var artwork = sender as Artwork;
            if (artwork == null)
            {
                return;
            }
            artwork.OnImageSourceChanged();
        }

        public Artwork()
        {
            this.InitializeComponent();
            this.SizeChanged += this.OnSizeChanged;
            this.Unloaded += this.OnUnloaded;
            if (PixelSize == null)
            {
                PixelSize = new Lazy<Size>(() => this.GetElementPixelSize());
            }
            if (PlaybackManager != null)
            {
                PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            }
            if (ThemeLoader != null)
            {
                ThemeLoader.ThemeChanged += this.OnThemeChanged;
            }
            var task = this.Refresh();
        }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var task = this.Refresh();
        }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= this.OnSizeChanged;
            this.Unloaded -= this.OnUnloaded;
            if (PlaybackManager != null)
            {
                PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
            if (ThemeLoader != null)
            {
                ThemeLoader.ThemeChanged -= this.OnThemeChanged;
            }
        }

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            var task = this.Refresh();
        }

        protected virtual void OnThemeChanged(object sender, AsyncEventArgs e)
        {
            var task = this.Refresh();
        }

        public bool ShowPlaceholder
        {
            get
            {
                return GetShowPlaceholder(this);
            }
            set
            {
                SetShowPlaceholder(this, value);
            }
        }

        protected virtual void OnShowPlaceholderChanged()
        {
            var task = this.Refresh();
        }

        public ImageSource ImageSource
        {
            get
            {
                return GetImageSource(this);
            }
            set
            {
                SetImageSource(this, value);
            }
        }

        protected virtual void OnImageSourceChanged()
        {

        }

        public int DecodePixelWidth
        {
            get
            {
                if (!PixelSize.IsValueCreated)
                {
                    if (this.ActualWidth == 0 || this.ActualHeight == 0)
                    {
                        return 0;
                    }
                }
                return (int)PixelSize.Value.Width;
            }
        }

        public int DecodePixelHeight
        {
            get
            {
                if (!PixelSize.IsValueCreated)
                {
                    if (this.ActualWidth == 0 || this.ActualHeight == 0)
                    {
                        return 0;
                    }
                }
                return (int)PixelSize.Value.Height;
            }
        }

        public async Task Refresh()
        {
            if (this.DecodePixelWidth == 0 || this.DecodePixelHeight == 0)
            {
                return;
            }
            var metaDataItem = default(MetaDataItem);
            var outputStream = PlaybackManager.CurrentStream;
            if (outputStream != null)
            {
                metaDataItem = await ArtworkProvider.Find(outputStream.PlaylistItem, ArtworkType.FrontCover);
                if (metaDataItem == null)
                {
                    metaDataItem = await ArtworkProvider.Find(outputStream.PlaylistItem.FileName, ArtworkType.FrontCover);
                }
            }
            if (metaDataItem == null || !File.Exists(metaDataItem.Value))
            {
                await Windows.Invoke(() =>
                {
                    if (this.ShowPlaceholder && ThemeLoader.Theme != null)
                    {
                        using (var stream = ThemeLoader.Theme.ArtworkPlaceholder)
                        {
                            if (this.Visibility != Visibility.Visible)
                            {
                                this.Visibility = Visibility.Visible;
                            }
                            this.ImageSource = ImageLoader.Load(stream, this.DecodePixelWidth, this.DecodePixelHeight);

                        }
                    }
                    else
                    {
                        this.Visibility = Visibility.Collapsed;
                        this.ImageSource = null;
                    }
                });
            }
            else
            {
                await Windows.Invoke(() =>
                {
                    if (this.Visibility != Visibility.Visible)
                    {
                        this.Visibility = Visibility.Visible;
                    }
                    this.ImageSource = ImageLoader.Load(metaDataItem.Value, this.DecodePixelWidth, this.DecodePixelHeight);
                });
            }
        }
    }
}
