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
        public static readonly ThemeLoader ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();

        public static readonly ImageLoader ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();

        public static readonly ILibraryHierarchyBrowser LibraryHierarchyBrowser = ComponentRegistry.Instance.GetComponent<ILibraryHierarchyBrowser>();

        public static readonly LibraryBrowserTileProvider LibraryBrowserTileProvider = ComponentRegistry.Instance.GetComponent<LibraryBrowserTileProvider>();

        public static readonly ISignalEmitter SignalEmitter = ComponentRegistry.Instance.GetComponent<ISignalEmitter>();

        public static readonly IConfiguration Configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();

        public static readonly DoubleConfigurationElement ScalingFactor = Configuration.GetElement<DoubleConfigurationElement>(
            WindowsUserInterfaceConfiguration.SECTION,
            WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
        );

        public static readonly IntegerConfigurationElement TileSize = Configuration.GetElement<IntegerConfigurationElement>(
            WindowsUserInterfaceConfiguration.SECTION,
            LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_TILE_SIZE
        );

        public static TaskFactory Factory { get; private set; }

        public static CappedDictionary<LibraryHierarchyNode, Lazy<ImageBrush>> Store { get; private set; }

        public static int TileWidth { get; private set; }

        public static int TileHeight { get; private set; }

        public static ResettableLazy<ImageBrush> FallbackValue { get; private set; }

        public static void InitializeComponent()
        {
            Configuration.GetElement<IntegerConfigurationElement>(
                ImageBehaviourConfiguration.SECTION,
                ImageLoaderConfiguration.THREADS
            ).ConnectValue(value => Factory = new TaskFactory(new TaskScheduler(new ParallelOptions()
            {
                MaxDegreeOfParallelism = value
            })));
            Configuration.GetElement<IntegerConfigurationElement>(
                ImageBehaviourConfiguration.SECTION,
                ImageLoaderConfiguration.CACHE_SIZE
            ).ConnectValue(value => Store = new CappedDictionary<LibraryHierarchyNode, Lazy<ImageBrush>>(value));
            SignalEmitter.Signal += (sender, signal) =>
            {
                switch (signal.Name)
                {
                    case CommonSignals.PluginInvocation:
                        switch (signal.State as string)
                        {
                            case ImageBehaviour.REFRESH_IMAGES:
                                FallbackValue.Reset();
                                Store.Clear();
                                return Windows.Invoke(() => CalculateTileSize());
                        }
                        break;
                }
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            };
            FallbackValue = new ResettableLazy<ImageBrush>(() =>
            {
                var source = ImageLoader.Load(
                    ThemeLoader.Theme.Id,
                    () => ThemeLoader.Theme.ArtworkPlaceholder,
                    TileWidth,
                    TileHeight,
                    true
                );
                var brush = new ImageBrush(source)
                {
                    Stretch = Stretch.Uniform
                };
                brush.Freeze();
                return brush;
            });
        }

        public static void CalculateTileSize()
        {
            var size = Windows.ActiveWindow.GetElementPixelSize(
                TileSize.Value * ScalingFactor.Value,
                TileSize.Value * ScalingFactor.Value
            );
            TileWidth = global::System.Convert.ToInt32(size.Width);
            TileHeight = global::System.Convert.ToInt32(size.Height);
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
            if (TileWidth == 0 || TileHeight == 0)
            {
                CalculateTileSize();
            }
            return new AsyncResult<ImageBrush>(FallbackValue.Value, Factory.StartNew(() =>
            {
                Store.Add(libraryHierarchyNode, new Lazy<ImageBrush>(() =>
                {
                    var source = LibraryBrowserTileProvider.CreateImageSource(libraryHierarchyNode, TileWidth, TileHeight, true);
                    if (source == null)
                    {
                        return FallbackValue.Value;
                    }
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
