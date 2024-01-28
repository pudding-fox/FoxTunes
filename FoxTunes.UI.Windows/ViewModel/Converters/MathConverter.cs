using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace FoxTunes.ViewModel
{
    public class MathConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = new Nullable<double>();
            foreach (var value in values.OfType<double>())
            {
                if (!result.HasValue)
                {
                    result = value;
                    continue;
                }
                switch (parameter as string)
                {
                    case "+":
                        result += value;
                        break;
                    case "-":
                        result -= value;
                        break;
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
