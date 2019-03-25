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

        public static readonly ArtworkGridProvider Provider = new ArtworkGridProvider();

        private static readonly ISignalEmitter SignalEmitter = ComponentRegistry.Instance.GetComponent<ISignalEmitter>();

        public static Lazy<Size> PixelSize { get; set; }

        static ArtworkGrid()
        {
            if (SignalEmitter != null)
            {
                SignalEmitter.Signal += (sender, e) =>
                {
                    switch (e.Name)
                    {
                        case CommonSignals.HierarchiesUpdated:
                            Provider.Clear();
                            break;
                    }
#if NET40
                    return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
                };
            }
        }

        public ArtworkGrid()
        {
            this.InitializeComponent();
            if (PixelSize == null)
            {
                PixelSize = new Lazy<Size>(() => this.GetElementPixelSize());
            }
        }

        public int DecodePixelWidth
        {
            get
            {
                return (int)PixelSize.Value.Width;
            }
        }

        public int DecodePixelHeight
        {
            get
            {
                return (int)PixelSize.Value.Height;
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
                var source = Provider.CreateImageSource(libraryHierarchyNode, width, height);
                await Windows.Invoke(() => this.Background = new ImageBrush(source)
                {
                    Stretch = Stretch.Uniform
                });
            });
        }
    }
}
