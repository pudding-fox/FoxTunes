using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ArtworkGrid.xaml
    /// </summary>
    public partial class ArtworkGrid : UserControl
    {
        public static TaskScheduler Scheduler = new TaskScheduler(new ParallelOptions()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        });

        public static TaskFactory Factory = new TaskFactory(Scheduler);

        public static Lazy<Size> PixelSize;

        public static readonly DoubleConfigurationElement ScalingFactor;

        public static readonly ArtworkGridProvider ArtworkGridProvider = ComponentRegistry.Instance.GetComponent<ArtworkGridProvider>();

        static ArtworkGrid()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration != null)
            {
                ScalingFactor = configuration.GetElement<DoubleConfigurationElement>(
                    WindowsUserInterfaceConfiguration.SECTION,
                    WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
                );
            }
            if (ArtworkGridProvider != null)
            {
                ArtworkGridProvider.Cleared += (sender, e) => PixelSize = null;
            }
        }

        public ArtworkGrid()
        {
            this.InitializeComponent();
        }

        public int DecodePixelWidth
        {
            get
            {
                this.EnsurePixelSize();
                return (int)(PixelSize.Value.Width * ScalingFactor.Value);
            }
        }

        public int DecodePixelHeight
        {
            get
            {
                this.EnsurePixelSize();
                return (int)(PixelSize.Value.Height * ScalingFactor.Value);
            }
        }

        protected virtual void EnsurePixelSize()
        {
            if (PixelSize == null)
            {
                PixelSize = new Lazy<Size>(() => this.GetElementPixelSize());
            }
        }

        public Task Refresh()
        {
            var libraryHierarchyNode = this.DataContext as LibraryHierarchyNode;
            if (this.Background != null || libraryHierarchyNode == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return Factory.StartNew(async () =>
            {
                if (!libraryHierarchyNode.IsMetaDatasLoaded)
                {
                    await libraryHierarchyNode.LoadMetaDatasAsync();
                }
                var width = default(int);
                var height = default(int);
                await Windows.Invoke(() =>
                {
                    width = this.DecodePixelWidth;
                    height = this.DecodePixelHeight;
                });
                var source = ArtworkGridProvider.CreateImageSource(libraryHierarchyNode, width, height);
                await Windows.Invoke(() => this.Background = new ImageBrush(source)
                {
                    Stretch = Stretch.Uniform
                });
            });
        }
    }
}
