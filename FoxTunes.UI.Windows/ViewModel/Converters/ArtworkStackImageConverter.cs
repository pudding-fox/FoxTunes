using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class ArtworkStackImageConverter : IValueConverter
    {
        public static readonly ArtworkStackBrushFactory Factory = ComponentRegistry.Instance.GetComponent<ArtworkStackBrushFactory>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fileName = value as string;
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return value;
            }
            return Factory.Create(fileName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
