using FoxTunes.Interfaces;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;

namespace FoxTunes.ViewModel
{
    public class ArtworkImageConverter : ViewModelBase, IValueConverter
    {
        public static readonly IArtworkProvider Provider = ComponentRegistry.Instance.GetComponent<IArtworkProvider>();

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width",
            typeof(double),
            typeof(ArtworkImageConverter),
            new PropertyMetadata(new PropertyChangedCallback(OnWidthChanged))
        );

        public static double GetWidth(ViewModelBase source)
        {
            return global::System.Convert.ToDouble(source.GetValue(WidthProperty));
        }

        public static void SetWidth(ViewModelBase source, double value)
        {
            source.SetValue(WidthProperty, value);
        }

        public static void OnWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var artworkImageConverter = sender as ArtworkImageConverter;
            if (artworkImageConverter == null)
            {
                return;
            }
            artworkImageConverter.OnWidthChanged();
        }

        public double Width
        {
            get
            {
                return global::System.Convert.ToDouble(this.GetValue(WidthProperty));
            }
            set
            {
                this.SetValue(WidthProperty, value);
            }
        }

        protected virtual void OnWidthChanged()
        {
            if (this.WidthChanged != null)
            {
                this.WidthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Width");
        }

        public event EventHandler WidthChanged;

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
            "Height",
            typeof(double),
            typeof(ArtworkImageConverter),
            new PropertyMetadata(new PropertyChangedCallback(OnHeightChanged))
        );

        public static double GetHeight(ViewModelBase source)
        {
            return global::System.Convert.ToDouble(source.GetValue(HeightProperty));
        }

        public static void SetHeight(ViewModelBase source, double value)
        {
            source.SetValue(HeightProperty, value);
        }

        public static void OnHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var artworkImageConverter = sender as ArtworkImageConverter;
            if (artworkImageConverter == null)
            {
                return;
            }
            artworkImageConverter.OnHeightChanged();
        }

        public double Height
        {
            get
            {
                return global::System.Convert.ToDouble(this.GetValue(HeightProperty));
            }
            set
            {
                this.SetValue(HeightProperty, value);
            }
        }

        protected virtual void OnHeightChanged()
        {
            if (this.HeightChanged != null)
            {
                this.HeightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Height");
        }

        public event EventHandler HeightChanged;

        public static readonly DependencyProperty ShowPlaceholderProperty = DependencyProperty.Register(
            "ShowPlaceholder",
            typeof(bool),
            typeof(ArtworkImageConverter),
            new PropertyMetadata(true, new PropertyChangedCallback(OnShowPlaceholderChanged))
        );

        public static bool GetShowPlaceholder(ViewModelBase source)
        {
            return global::System.Convert.ToBoolean(source.GetValue(ShowPlaceholderProperty));
        }

        public static void SetShowPlaceholder(ViewModelBase source, bool value)
        {
            source.SetValue(ShowPlaceholderProperty, value);
        }

        public static void OnShowPlaceholderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var artworkImageConverter = sender as ArtworkImageConverter;
            if (artworkImageConverter == null)
            {
                return;
            }
            artworkImageConverter.OnShowPlaceholderChanged();
        }

        public bool ShowPlaceholder
        {
            get
            {
                return global::System.Convert.ToBoolean(this.GetValue(ShowPlaceholderProperty));
            }
            set
            {
                this.SetValue(ShowPlaceholderProperty, value);
            }
        }

        protected virtual void OnShowPlaceholderChanged()
        {
            if (this.ShowPlaceholderChanged != null)
            {
                this.ShowPlaceholderChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowPlaceholder");
        }

        public event EventHandler ShowPlaceholderChanged;

        public ArtworkBrushFactory ArtworkBrushFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ArtworkBrushFactory = new ArtworkBrushFactory();
            this.ArtworkBrushFactory.InitializeComponent(core);
            base.InitializeComponent(core);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Provider == null || this.ArtworkBrushFactory == null)
            {
                return null;
            }
            if (double.IsNaN(this.Width) || double.IsNaN(this.Height) || this.Width == 0 || this.Height == 0)
            {
                return null;
            }
            var fileName = default(string);
            if (value is string)
            {
                fileName = (string)value;
                if (!this.ShowPlaceholder && !FileSystemHelper.IsLocalPath(fileName))
                {
                    return null;
                }
            }
            else if (value is IFileData)
            {
                fileName = Provider.Find(
                    (IFileData)value,
                    ArtworkType.FrontCover
                );
            }
            var size = global::System.Convert.ToInt32(Math.Max(this.Width, this.Height));
            return this.ArtworkBrushFactory.Create(
                fileName,
                size,
                size
            );
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ArtworkImageConverter();
        }
    }
}
