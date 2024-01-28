using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FoxTunes
{
    public class ArtworkStackBrushFactory : StandardFactory
    {
        const int TILE_SIZE = 100;

        public ArtworkStackBrushFactory()
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

        public ThemeLoader ThemeLoader { get; private set; }

        public ImageLoader ImageLoader { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public DoubleConfigurationElement ScalingFactor { get; private set; }

        public int TileWidth { get; private set; }

        public int TileHeight { get; private set; }

        public TaskFactory Factory { get; private set; }

        public ResettableLazy<ImageBrush> FallbackValue { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            this.ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.Configuration.GetElement<IntegerConfigurationElement>(
                ImageBehaviourConfiguration.SECTION,
                ImageLoaderConfiguration.THREADS
            ).ConnectValue(value => this.CreateTaskFactory(value));
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
                            return this.Reset();
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public AsyncResult<ImageBrush> Create(string fileName)
        {
            if (this.TileWidth == 0 || this.TileHeight == 0)
            {
                this.CalculateTileSize();
            }
            return new AsyncResult<ImageBrush>(this.FallbackValue.Value, this.Factory.StartNew(() =>
            {
                var source = ImageLoader.Load(
                    fileName,
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
            }));
        }

        protected virtual void CreateTaskFactory(int threads)
        {
            this.Factory = new TaskFactory(new TaskScheduler(new ParallelOptions()
            {
                MaxDegreeOfParallelism = threads
            }));
        }

        protected virtual void CalculateTileSize()
        {
            var size = Windows.ActiveWindow.GetElementPixelSize(
                TILE_SIZE * this.ScalingFactor.Value,
                TILE_SIZE * this.ScalingFactor.Value
            );
            this.TileWidth = global::System.Convert.ToInt32(size.Width);
            this.TileHeight = global::System.Convert.ToInt32(size.Height);
        }

        protected virtual Task Reset()
        {
            this.FallbackValue.Reset();
            return Windows.Invoke(() => this.CalculateTileSize());
        }
    }
}
