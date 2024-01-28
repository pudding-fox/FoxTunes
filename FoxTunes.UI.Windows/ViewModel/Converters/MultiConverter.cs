using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class MultiConverter : List<object>, IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value;
            foreach (var element in this)
            {
                if (element is IValueConverter)
                {
                    var converter = element as IValueConverter;
                    result = converter.Convert(result, targetType, parameter, culture);
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = (object)values;
            foreach (var element in this)
            {
                if (element is IValueConverter)
                {
                    var converter = element as IValueConverter;
                    result = converter.Convert(result, targetType, parameter, culture);
                }
                else if (element is IMultiValueConverter && result is object[])
                {
                    var converter = element as IMultiValueConverter;
                    result = converter.Convert(result as object[], targetType, parameter, culture);
                }
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
