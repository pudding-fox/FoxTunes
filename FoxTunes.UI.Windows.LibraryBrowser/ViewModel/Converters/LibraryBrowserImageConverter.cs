using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class LibraryBrowserImageConverter : ViewModelBase, IValueConverter
    {
        public static readonly LibraryBrowserTileBrushFactory Factory = ComponentRegistry.Instance.GetComponent<LibraryBrowserTileBrushFactory>();

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width",
            typeof(double),
            typeof(LibraryBrowserImageConverter),
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
            var libraryBrowserImageConverter = sender as LibraryBrowserImageConverter;
            if (libraryBrowserImageConverter == null)
            {
                return;
            }
            libraryBrowserImageConverter.OnWidthChanged();
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
            typeof(LibraryBrowserImageConverter),
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
            var libraryBrowserImageConverter = sender as LibraryBrowserImageConverter;
            if (libraryBrowserImageConverter == null)
            {
                return;
            }
            libraryBrowserImageConverter.OnHeightChanged();
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

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Factory == null)
            {
                return null;
            }
            if (double.IsNaN(this.Width) || double.IsNaN(this.Height) || this.Width == 0 || this.Height == 0)
            {
                return null;
            }
            if (value is LibraryHierarchyNode libraryHierarchyNode)
            {
                var size = global::System.Convert.ToInt32(Math.Max(this.Width, this.Height));
                return Factory.Create(
                    libraryHierarchyNode,
                    size,
                    size
                );
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryBrowserImageConverter();
        }
    }
}
