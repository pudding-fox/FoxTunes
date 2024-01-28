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
                width = size[0];
                height = size[1];
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
