using FoxTunes.Interfaces;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace FoxTunes.ViewModel
{
    public class LibraryBrowserImageConverter : IValueConverter
    {
        public static readonly ILibraryHierarchyBrowser LibraryHierarchyBrowser = ComponentRegistry.Instance.GetComponent<ILibraryHierarchyBrowser>();

        public static readonly LibraryBrowserTileProvider LibraryBrowserTileProvider = ComponentRegistry.Instance.GetComponent<LibraryBrowserTileProvider>();

        public static readonly ISignalEmitter SignalEmitter = ComponentRegistry.Instance.GetComponent<ISignalEmitter>();

        public static CappedDictionary<LibraryHierarchyNode, Lazy<ImageBrush>> Store { get; private set; }

        public static int TileWidth { get; private set; }

        public static int TileHeight { get; private set; }

        static LibraryBrowserImageConverter()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration == null)
            {
                return;
            }
            configuration.GetElement<IntegerConfigurationElement>(
                ImageBehaviourConfiguration.SECTION,
                ImageLoaderConfiguration.CACHE_SIZE
            ).ConnectValue(value => Store = new CappedDictionary<LibraryHierarchyNode, Lazy<ImageBrush>>(value));
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
                TileWidth = global::System.Convert.ToInt32(size.Width);
                TileHeight = global::System.Convert.ToInt32(size.Height);
            });
            scalingFactor.ValueChanged += handler;
            tileSize.ValueChanged += handler;
            handler(typeof(LibraryBrowserImageConverter), EventArgs.Empty);
            SignalEmitter.Signal += (sender, signal) =>
            {
                switch (signal.Name)
                {
                    case CommonSignals.PluginInvocation:
                        switch (signal.State as string)
                        {
                            case ImageBehaviour.REFRESH_IMAGES:
                                Store.Clear();
                                break;
                        }
                        break;
                }
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is LibraryHierarchyNode libraryHierarchyNode))
            {
                return value;
            }
            var cachedBrush = default(Lazy<ImageBrush>);
            if (Store.TryGetValue(libraryHierarchyNode, out cachedBrush))
            {
                return AsyncResult<ImageBrush>.FromValue(cachedBrush.Value);
            }
#if NET40
            return new AsyncResult<ImageBrush>(TaskEx.Run(() =>
#else
            return new AsyncResult<ImageBrush>(Task.Run(() =>
#endif
            {
                Store.Add(libraryHierarchyNode, new Lazy<ImageBrush>(() =>
                {
                    var source = LibraryBrowserTileProvider.CreateImageSource(libraryHierarchyNode, TileWidth, TileHeight, true);
                    var brush = new ImageBrush(source)
                    {
                        Stretch = Stretch.Uniform
                    };
                    brush.Freeze();
                    return brush;
                }));
                Store.TryGetValue(libraryHierarchyNode, out cachedBrush);
                return cachedBrush.Value;
            }));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
