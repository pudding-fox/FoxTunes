using System;
using System.Globalization;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class PlaybackCaptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPlaying)
            {
                if (isPlaying)
                {
                    return ";";
                }
                else
                {
                    return "4";
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
