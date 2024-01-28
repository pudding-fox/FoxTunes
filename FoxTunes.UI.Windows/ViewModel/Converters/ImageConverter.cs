using FoxTunes.Interfaces;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;

namespace FoxTunes.ViewModel
{
    public class ImageConverter : IValueConverter
    {
        public static readonly ImageLoader ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();

        public static int WidthFactor { get; private set; }

        public static int HeightFactor { get; private set; }

        static ImageConverter()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration == null)
            {
                return;
            }
            var scalingFactor = configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            if (scalingFactor == null)
            {
                return;
            }
            var handler = new EventHandler((sender, e) =>
            {
                var size = Windows.ActiveWindow.GetElementPixelSize(
                    scalingFactor.Value,
                    scalingFactor.Value
                );
                WidthFactor = global::System.Convert.ToInt32(size.Width);
                HeightFactor = global::System.Convert.ToInt32(size.Height);
            });
            scalingFactor.ValueChanged += handler;
            handler(typeof(ImageConverter), EventArgs.Empty);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fileName = value as string;
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return value;
            }
            var width = 0;
            var height = 0;
            var size = parameter as int[];
            if (size != null && size.Length == 2)
            {
                width = size[0] * Math.Max(WidthFactor, 1);
                height = size[1] * Math.Max(HeightFactor, 1);
            }
            var source = ImageLoader.Load(
                fileName,
                width,
                height,
                true
            );
            var brush = new ImageBrush(source)
            {
                Stretch = Stretch.Uniform
            };
            brush.Freeze();
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
