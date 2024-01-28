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

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource",
            typeof(ImageSource),
            typeof(ArtworkGrid),
            new FrameworkPropertyMetadata(default(ImageSource), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnImageSourceChanged))
        );

        public static ImageSource GetImageSource(ArtworkGrid source)
        {
            return (ImageSource)source.GetValue(ImageSourceProperty);
        }

        public static void SetImageSource(ArtworkGrid source, ImageSource value)
        {
            source.SetValue(ImageSourceProperty, value);
        }

        private static void OnImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var artworkGrid = sender as ArtworkGrid;
            if (artworkGrid == null)
            {
                return;
            }
            artworkGrid.OnImageSourceChanged();
        }

        public ArtworkGrid()
        {
            this.InitializeComponent();
            if (PixelSize == null)
            {
                PixelSize = new Lazy<Size>(() => this.GetElementPixelSize());
            }
            this.SizeChanged += this.OnSizeChanged;
            this.Unloaded += this.OnUnloaded;
        }

        public ImageSource ImageSource
        {
            get
            {
                return GetImageSource(this);
            }
            set
            {
                SetImageSource(this, value);
            }
        }

        protected virtual void OnImageSourceChanged()
        {

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

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var libraryHierarchyNode = this.DataContext as LibraryHierarchyNode;
            if (libraryHierarchyNode == null)
            {
                return;
            }
            var task = this.Refresh(libraryHierarchyNode);
        }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= this.OnSizeChanged;
            this.Unloaded -= this.OnUnloaded;
        }

        public Task Refresh(LibraryHierarchyNode libraryHierarchyNode)
        {
            return Factory.StartNew(async () =>
            {
                if (!libraryHierarchyNode.IsMetaDatasLoaded)
                {
                    await libraryHierarchyNode.LoadMetaDatasAsync();
                }
                await Windows.Invoke(() => this.ImageSource = Provider.CreateImageSource(libraryHierarchyNode, this.DecodePixelWidth, this.DecodePixelHeight));
            });
        }
    }
}
