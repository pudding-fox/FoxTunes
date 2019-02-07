using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class WindowStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is WindowState))
            {
                throw new NotImplementedException();
            }
            switch ((WindowState)value)
            {
                case WindowState.Maximized:
                    return "\u0032";
                default:
                    return "\u0031";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
