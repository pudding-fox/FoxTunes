using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FoxTunes
{
    public class LibraryBrowserTileBrushFactory : StandardFactory
    {
        public LibraryBrowserTileBrushFactory()
        {
            this.FallbackValue = new ResettableLazy<ImageBrush>(() =>
            {
                if (this.TileWidth == 0 || this.TileHeight == 0)
                {
                    this.CalculateTileSize();
                }
                var source = ImageLoader.Load(
                    this.ThemeLoader.Theme.Id,
                    () => this.ThemeLoader.Theme.ArtworkPlaceholder,
                    this.TileWidth,
                    this.TileHeight,
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

        public LibraryBrowserTileProvider LibraryBrowserTileProvider { get; private set; }

        public ThemeLoader ThemeLoader { get; private set; }

        public ImageLoader ImageLoader { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public DoubleConfigurationElement ScalingFactor { get; private set; }

        public IntegerConfigurationElement TileSize { get; private set; }

        public TaskFactory Factory { get; private set; }

        public CappedDictionary<LibraryHierarchyNode, ImageBrush> Store { get; private set; }

        public int TileWidth { get; private set; }

        public int TileHeight { get; private set; }

        public ResettableLazy<ImageBrush> FallbackValue { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryBrowserTileProvider = ComponentRegistry.Instance.GetComponent<LibraryBrowserTileProvider>();
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            this.ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.TileSize = this.Configuration.GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_TILE_SIZE
            );
            this.Configuration.GetElement<IntegerConfigurationElement>(
                ImageBehaviourConfiguration.SECTION,
                ImageLoaderConfiguration.THREADS
            ).ConnectValue(value => this.CreateTaskFactory(value));
            this.Configuration.GetElement<IntegerConfigurationElement>(
                ImageBehaviourConfiguration.SECTION,
                ImageLoaderConfiguration.CACHE_SIZE
            ).ConnectValue(value => this.CreateCache(value));
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PluginInvocation:
                    switch (signal.State as string)
                    {
                        case ImageBehaviour.REFRESH_IMAGES:
                            this.FallbackValue.Reset();
                            this.Store.Clear();
                            return Windows.Invoke(() => this.CalculateTileSize());
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public AsyncResult<ImageBrush> Create(LibraryHierarchyNode libraryHierarchyNode)
        {
            var cache = string.IsNullOrEmpty(this.LibraryHierarchyBrowser.Filter);
            if (cache)
            {
                var brush = default(ImageBrush);
                if (this.Store.TryGetValue(libraryHierarchyNode, out brush))
                {
                    return AsyncResult<ImageBrush>.FromValue(brush);
                }
            }
            if (this.TileWidth == 0 || this.TileHeight == 0)
            {
                this.CalculateTileSize();
            }
            return new AsyncResult<ImageBrush>(this.FallbackValue.Value, this.Factory.StartNew(() =>
            {
                if (cache)
                {
                    return this.Store.GetOrAdd(
                        libraryHierarchyNode,
                        () => this.Create(libraryHierarchyNode, true)
                    );
                }
                else
                {
                    return this.Create(libraryHierarchyNode, false);
                }
            }));
        }

        protected virtual ImageBrush Create(LibraryHierarchyNode libraryHierarchyNode, bool cache)
        {
            var source = this.LibraryBrowserTileProvider.CreateImageSource(
                libraryHierarchyNode,
                this.TileWidth,
                this.TileHeight,
                cache
            );
            if (source == null)
            {
                return this.FallbackValue.Value;
            }
            var brush = new ImageBrush(source)
            {
                Stretch = Stretch.Uniform
            };
            brush.Freeze();
            return brush;
        }

        protected virtual void CreateTaskFactory(int threads)
        {
            this.Factory = new TaskFactory(new TaskScheduler(new ParallelOptions()
            {
                MaxDegreeOfParallelism = threads
            }));
        }

        protected virtual void CreateCache(int capacity)
        {
            this.Store = new CappedDictionary<LibraryHierarchyNode, ImageBrush>(capacity);
        }

        protected virtual void CalculateTileSize()
        {
            var size = Windows.ActiveWindow.GetElementPixelSize(
                this.TileSize.Value * this.ScalingFactor.Value,
                this.TileSize.Value * this.ScalingFactor.Value
            );
            this.TileWidth = global::System.Convert.ToInt32(size.Width);
            this.TileHeight = global::System.Convert.ToInt32(size.Height);
        }

        protected override void OnDisposing()
        {
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.OnDisposing();
        }
    }
}
