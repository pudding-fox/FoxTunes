using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class ImageConverter : IValueConverter
    {
        public static readonly ImageBrushFactory Factory = ComponentRegistry.Instance.GetComponent<ImageBrushFactory>();

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
            return Factory.Create(fileName, width, height);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
