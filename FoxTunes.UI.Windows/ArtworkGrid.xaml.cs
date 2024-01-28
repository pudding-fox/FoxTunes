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
        private static readonly ThemeLoader ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();

        private static readonly ImageSourceConverter ImageSourceConverter = new ImageSourceConverter();

        public static readonly DependencyProperty ImageSource0Property = DependencyProperty.Register(
            "ImageSource0",
            typeof(ImageSource),
            typeof(ArtworkGrid),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnImageSource0Changed))
        );

        public static ImageSource GetImageSource0(ArtworkGrid source)
        {
            return (ImageSource)source.GetValue(ImageSource0Property);
        }

        public static void SetImageSource0(ArtworkGrid source, ImageSource value)
        {
            source.SetValue(ImageSource0Property, value);
        }

        private static void OnImageSource0Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var artworkGrid = sender as ArtworkGrid;
            if (artworkGrid == null)
            {
                return;
            }
            artworkGrid.OnImageSource0Changed();
        }

        public static readonly DependencyProperty ImageSource1Property = DependencyProperty.Register(
            "ImageSource1",
            typeof(ImageSource),
            typeof(ArtworkGrid),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnImageSource1Changed))
        );

        public static ImageSource GetImageSource1(ArtworkGrid source)
        {
            return (ImageSource)source.GetValue(ImageSource1Property);
        }

        public static void SetImageSource1(ArtworkGrid source, ImageSource value)
        {
            source.SetValue(ImageSource1Property, value);
        }

        private static void OnImageSource1Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var artworkGrid = sender as ArtworkGrid;
            if (artworkGrid == null)
            {
                return;
            }
            artworkGrid.OnImageSource1Changed();
        }

        public static readonly DependencyProperty ImageSource2Property = DependencyProperty.Register(
            "ImageSource2",
            typeof(ImageSource),
            typeof(ArtworkGrid),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnImageSource2Changed))
        );

        public static ImageSource GetImageSource2(ArtworkGrid source)
        {
            return (ImageSource)source.GetValue(ImageSource2Property);
        }

        public static void SetImageSource2(ArtworkGrid source, ImageSource value)
        {
            source.SetValue(ImageSource2Property, value);
        }

        private static void OnImageSource2Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var artworkGrid = sender as ArtworkGrid;
            if (artworkGrid == null)
            {
                return;
            }
            artworkGrid.OnImageSource2Changed();
        }

        public static readonly DependencyProperty ImageSource3Property = DependencyProperty.Register(
            "ImageSource3",
            typeof(ImageSource),
            typeof(ArtworkGrid),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnImageSource3Changed))
        );

        public static ImageSource GetImageSource3(ArtworkGrid source)
        {
            return (ImageSource)source.GetValue(ImageSource3Property);
        }

        public static void SetImageSource3(ArtworkGrid source, ImageSource value)
        {
            source.SetValue(ImageSource3Property, value);
        }

        private static void OnImageSource3Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var artworkGrid = sender as ArtworkGrid;
            if (artworkGrid == null)
            {
                return;
            }
            artworkGrid.OnImageSource3Changed();
        }

        public static readonly DependencyProperty ImageSource4Property = DependencyProperty.Register(
            "ImageSource4",
            typeof(ImageSource),
            typeof(ArtworkGrid),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnImageSource4Changed))
        );

        public static ImageSource GetImageSource4(ArtworkGrid source)
        {
            return (ImageSource)source.GetValue(ImageSource4Property);
        }

        public static void SetImageSource4(ArtworkGrid source, ImageSource value)
        {
            source.SetValue(ImageSource4Property, value);
        }

        private static void OnImageSource4Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var artworkGrid = sender as ArtworkGrid;
            if (artworkGrid == null)
            {
                return;
            }
            artworkGrid.OnImageSource4Changed();
        }

        public ArtworkGrid()
        {
            this.InitializeComponent();
            this.DataContextChanged += this.OnDataContextChanged;
        }

        protected virtual void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var libraryHierarchyNode = this.DataContext as LibraryHierarchyNode;
            if (libraryHierarchyNode == null || libraryHierarchyNode.MetaDatas == null)
            {
                return;
            }
            switch (libraryHierarchyNode.MetaDatas.Count)
            {
                case 0:
                    this.Load0();
                    break;
                case 1:
                    this.Load1(libraryHierarchyNode);
                    break;
                case 2:
                    this.Load2(libraryHierarchyNode);
                    break;
                case 3:
                    this.Load3(libraryHierarchyNode);
                    break;
                default:
                    this.Load4(libraryHierarchyNode);
                    break;
            }
        }

        private void Load0()
        {
            if (ThemeLoader != null && ThemeLoader.Theme != null)
            {
                using (var stream = ThemeLoader.Theme.ArtworkPlaceholder)
                {
                    this.ImageSource0 = (ImageSource)ImageSourceConverter.ConvertFrom(stream);
                }
            }
        }

        private void Load1(LibraryHierarchyNode libraryHierarchyNode)
        {
            this.ImageSource1 = (ImageSource)ImageSourceConverter.ConvertFrom(libraryHierarchyNode.MetaDatas[0].FileValue);
        }

        private void Load2(LibraryHierarchyNode libraryHierarchyNode)
        {
            this.ImageSource1 = (ImageSource)ImageSourceConverter.ConvertFrom(libraryHierarchyNode.MetaDatas[0].FileValue);
            this.ImageSource2 = (ImageSource)ImageSourceConverter.ConvertFrom(libraryHierarchyNode.MetaDatas[1].FileValue);
        }

        private void Load3(LibraryHierarchyNode libraryHierarchyNode)
        {
            this.ImageSource1 = (ImageSource)ImageSourceConverter.ConvertFrom(libraryHierarchyNode.MetaDatas[0].FileValue);
            this.ImageSource2 = (ImageSource)ImageSourceConverter.ConvertFrom(libraryHierarchyNode.MetaDatas[1].FileValue);
            this.ImageSource3 = (ImageSource)ImageSourceConverter.ConvertFrom(libraryHierarchyNode.MetaDatas[2].FileValue);
        }

        private void Load4(LibraryHierarchyNode libraryHierarchyNode)
        {
            this.ImageSource1 = (ImageSource)ImageSourceConverter.ConvertFrom(libraryHierarchyNode.MetaDatas[0].FileValue);
            this.ImageSource2 = (ImageSource)ImageSourceConverter.ConvertFrom(libraryHierarchyNode.MetaDatas[1].FileValue);
            this.ImageSource3 = (ImageSource)ImageSourceConverter.ConvertFrom(libraryHierarchyNode.MetaDatas[2].FileValue);
            this.ImageSource4 = (ImageSource)ImageSourceConverter.ConvertFrom(libraryHierarchyNode.MetaDatas[3].FileValue);
        }

        public ImageSource ImageSource0
        {
            get
            {
                return GetImageSource0(this);
            }
            set
            {
                SetImageSource0(this, value);
            }
        }

        protected virtual void OnImageSource0Changed()
        {
            //Nothing to do.
        }

        public ImageSource ImageSource1
        {
            get
            {
                return GetImageSource1(this);
            }
            set
            {
                SetImageSource1(this, value);
            }
        }

        protected virtual void OnImageSource1Changed()
        {
            //Nothing to do.
        }

        public ImageSource ImageSource2
        {
            get
            {
                return GetImageSource2(this);
            }
            set
            {
                SetImageSource2(this, value);
            }
        }

        protected virtual void OnImageSource2Changed()
        {
            //Nothing to do.
        }

        public ImageSource ImageSource3
        {
            get
            {
                return GetImageSource3(this);
            }
            set
            {
                SetImageSource3(this, value);
            }
        }

        protected virtual void OnImageSource3Changed()
        {
            //Nothing to do.
        }

        public ImageSource ImageSource4
        {
            get
            {
                return GetImageSource4(this);
            }
            set
            {
                SetImageSource4(this, value);
            }
        }

        protected virtual void OnImageSource4Changed()
        {
            //Nothing to do.
        }
    }
}
