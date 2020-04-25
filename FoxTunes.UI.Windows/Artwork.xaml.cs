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
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var task = this.Refresh();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Background = null;
        }

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(() => this.Refresh());
#else
            var task = Task.Run(() => this.Refresh());
#endif
        }

        protected virtual void OnThemeChanged(object sender, EventArgs e)
        {
            var task = this.Refresh();
        }

        public async Task Refresh()
        {
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
                    0,
                    0,
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
