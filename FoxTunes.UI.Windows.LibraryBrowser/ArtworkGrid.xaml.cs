using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ArtworkGrid.xaml
    /// </summary>
    public partial class ArtworkGrid : UserControl
    {
        public static readonly ILibraryHierarchyBrowser LibraryHierarchyBrowser = ComponentRegistry.Instance.GetComponent<ILibraryHierarchyBrowser>();

        public static readonly ArtworkGridProvider ArtworkGridProvider = ComponentRegistry.Instance.GetComponent<ArtworkGridProvider>();

        public static int TileWidth { get; private set; }

        public static int TileHeight { get; private set; }

        static ArtworkGrid()
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
            handler(typeof(ArtworkGrid), EventArgs.Empty);
        }

        public ArtworkGrid()
        {
            this.InitializeComponent();
        }

        public async Task Refresh()
        {
            var libraryHierarchyNode = default(LibraryHierarchyNode);
            await Windows.Invoke(() =>
            {
                libraryHierarchyNode = this.DataContext as LibraryHierarchyNode;
            }).ConfigureAwait(false);
            if (libraryHierarchyNode == null)
            {
                //Very rare.
                return;
            }
            var cache = libraryHierarchyNode.IsLeaf || string.IsNullOrEmpty(LibraryHierarchyBrowser.Filter);
            var source = ArtworkGridProvider.CreateImageSource(libraryHierarchyNode, TileWidth, TileHeight, cache);
            var brush = new ImageBrush(source)
            {
                Stretch = Stretch.Uniform
            };
            brush.Freeze();
            await Windows.Invoke(() => this.Background = brush).ConfigureAwait(false);
        }
    }
}
