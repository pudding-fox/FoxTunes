using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

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

        private MetaDataItem _Image { get; set; }

        public MetaDataItem Image
        {
            get
            {
                if (this._Image == null && this.ShowPlaceholder)
                {
                    return new MetaDataItem(Enum.GetName(typeof(ArtworkType), ArtworkType.FrontCover), MetaDataItemType.Image)
                    {
                        FileValue = this.ThemeLoader.Theme.ArtworkPlaceholder
                    };
                }
                return this._Image;
            }
            set
            {
                this._Image = value;
                this.OnImageChanged();
            }
        }

        protected virtual void OnImageChanged()
        {
            if (this.ImageChanged != null)
            {
                this.ImageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Image");
        }

        public event EventHandler ImageChanged = delegate { };

        public Task Refresh()
        {
            if (this.PlaylistManager == null)
            {
                return Task.CompletedTask;
            }
            var image = default(MetaDataItem);
            var playlistItem = this.PlaylistManager.CurrentItem;
            if (playlistItem != null)
            {
                image = this.ArtworkProvider.Find(playlistItem, ArtworkType.FrontCover);
                if (image == null)
                {
                    image = this.ArtworkProvider.Find(playlistItem.FileName, ArtworkType.FrontCover);
                }
            }
            return Windows.Invoke(() => this.Image = image);
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
            if (this._Image == null)
            {
                using (e.Defer())
                {
                    await Windows.Invoke(new Action(this.OnImageChanged));
                }
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
