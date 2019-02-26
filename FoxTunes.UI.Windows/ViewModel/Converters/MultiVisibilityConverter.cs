using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class MultiVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = Visibility.Visible;
            foreach(var value in values)
            {
                if(!(value is Visibility))
                {
                    continue;
                }
                if (visibility != (Visibility)value)
                {
                    visibility = (Visibility)value;
                }
            }
            return visibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
