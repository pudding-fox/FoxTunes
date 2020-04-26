using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for LibraryBrowserTile.xaml
    /// </summary>
    public partial class LibraryBrowserTile : UserControl
    {
        public static readonly ILibraryHierarchyBrowser LibraryHierarchyBrowser = ComponentRegistry.Instance.GetComponent<ILibraryHierarchyBrowser>();

        public static readonly LibraryBrowserTileProvider LibraryBrowserTileProvider = ComponentRegistry.Instance.GetComponent<LibraryBrowserTileProvider>();

        public static int TileWidth { get; private set; }

        public static int TileHeight { get; private set; }

        static LibraryBrowserTile()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration == null)
            {
                return;
            }
            var scalingFactor = configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            var tileSize = configuration.GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_TILE_SIZE
            );
            if (scalingFactor == null || tileSize == null)
            {
                return;
            }
            var handler = new EventHandler((sender, e) =>
            {
                var size = Windows.ActiveWindow.GetElementPixelSize(
                    tileSize.Value * scalingFactor.Value,
                    tileSize.Value * scalingFactor.Value
                );
                TileWidth = Convert.ToInt32(size.Width);
                TileHeight = Convert.ToInt32(size.Height);
            });
            scalingFactor.ValueChanged += handler;
            tileSize.ValueChanged += handler;
            handler(typeof(LibraryBrowserTile), EventArgs.Empty);
        }

        public LibraryBrowserTile()
        {
            this.InitializeComponent();
        }

        public async Task Refresh()
        {
            var cancel = default(bool);
            var libraryHierarchyNode = default(LibraryHierarchyNode);
            await Windows.Invoke(() =>
            {
                if (this.Background != null)
                {
                    cancel = true;
                    return;
                }
                libraryHierarchyNode = this.DataContext as LibraryHierarchyNode;
            }).ConfigureAwait(false);
            if (cancel)
            {
                //Already loaded.
                return;
            }
            if (libraryHierarchyNode == null)
            {
                //Very rare.
                return;
            }
            var source = LibraryBrowserTileProvider.CreateImageSource(libraryHierarchyNode, TileWidth, TileHeight, true);
            var brush = new ImageBrush(source)
            {
                Stretch = Stretch.Uniform
            };
            brush.Freeze();
            await Windows.Invoke(() => this.Background = brush).ConfigureAwait(false);
        }
    }
}
