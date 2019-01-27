using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes.ViewModel
{
    public class Artwork : ViewModelBase
    {
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
            var marquee = sender as Artwork;
            if (marquee == null)
            {
                return;
            }
            marquee.OnShowPlaceholderChanged();
        }

        public IArtworkProvider ArtworkProvider { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ThemeLoader ThemeLoader { get; private set; }

        public IConfiguration Configuration { get; private set; }

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
            //Nothing to do.
        }

        private ImageSource _ImageSource { get; set; }

        public ImageSource ImageSource
        {
            get
            {
                return this._ImageSource;
            }
            set
            {
                this._ImageSource = value;
                this.OnImageSourceChanged();
            }
        }

        protected virtual void OnImageSourceChanged()
        {
            if (this.ImageSourceChanged != null)
            {
                this.ImageSourceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ImageSource");
        }

        public event EventHandler ImageSourceChanged = delegate { };

        public async Task Refresh()
        {
            if (this.PlaylistManager == null)
            {
                return;
            }
            var metaDataItem = default(MetaDataItem);
            var playlistItem = this.PlaylistManager.CurrentItem;
            if (playlistItem != null)
            {
                metaDataItem = await this.ArtworkProvider.Find(playlistItem, ArtworkType.FrontCover);
                if (metaDataItem == null)
                {
                    metaDataItem = await this.ArtworkProvider.Find(playlistItem.FileName, ArtworkType.FrontCover);
                }
            }
            if (metaDataItem == null || !File.Exists(metaDataItem.FileValue))
            {
                await Windows.Invoke(() =>
                {
                    if (this.ShowPlaceholder && this.ThemeLoader.Theme != null)
                    {
                        this.ImageSource = this.LoadImage(this.ThemeLoader.Theme.ArtworkPlaceholder);
                    }
                    else
                    {
                        this.ImageSource = null;
                    }
                });
            }
            else
            {
                await Windows.Invoke(() => this.ImageSource = this.LoadImage(metaDataItem.FileValue));
            }
        }

        protected virtual ImageSource LoadImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(fileName);
            //TODO: I don't know if we can use caching, there doesn't appear to be a way to set the cache capacity?
            image.CacheOption = BitmapCacheOption.None;
            image.EndInit();
            return image;
        }

        protected virtual ImageSource LoadImage(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }

        public override void InitializeComponent(ICore core)
        {
            this.ArtworkProvider = this.Core.Components.ArtworkProvider;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaylistManager.CurrentItemChanged += this.OnCurrentItemChanged;
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            this.ThemeLoader.ThemeChanged += this.OnThemeChanged;
            var task = this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual async void OnCurrentItemChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await this.Refresh();
            }
        }

        protected virtual async void OnThemeChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await this.Refresh();
            }
        }

        protected override void OnDisposing()
        {
            if (this.PlaylistManager != null)
            {
                this.PlaylistManager.CurrentItemChanged -= this.OnCurrentItemChanged;
            }
            if (this.ThemeLoader != null)
            {
                this.ThemeLoader.ThemeChanged -= this.OnThemeChanged;
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Artwork();
        }
    }
}
