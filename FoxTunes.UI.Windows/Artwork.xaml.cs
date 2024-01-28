using FoxTunes.Interfaces;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Artwork.xaml
    /// </summary>
    [UIComponent("66C8A9E7-0891-48DD-8086-E40F72D4D030", UIComponentSlots.NONE, "Artwork")]
    public partial class Artwork : Square
    {
        public static readonly IArtworkProvider ArtworkProvider = ComponentRegistry.Instance.GetComponent<IArtworkProvider>();

        public static readonly IPlaybackManager PlaybackManager = ComponentRegistry.Instance.GetComponent<IPlaybackManager>();

        public static readonly ThemeLoader ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();

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
            var task = this.Refresh();
        }

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            var task = this.Refresh();
        }

        protected virtual void OnThemeChanged(object sender, AsyncEventArgs e)
        {
            var task = this.Refresh();
        }

        public async Task Refresh()
        {
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
                    using (var stream = ThemeLoader.Theme.ArtworkPlaceholder)
                    {
                        this.Background = new ImageBrush(ImageLoader.Load(stream, 0, 0))
                        {
                            Stretch = Stretch.Uniform
                        };
                    }
                    this.IsComponentEnabled = false;
                });
            }
            else
            {
                await Windows.Invoke(() =>
                {
                    this.Background = new ImageBrush(ImageLoader.Load(metaDataItem.Value, 0, 0))
                    {
                        Stretch = Stretch.Uniform
                    };
                    this.IsComponentEnabled = true;
                });
            }
        }
    }
}
