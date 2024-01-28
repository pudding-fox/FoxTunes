using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FoxTunes.ViewModel
{
    public class SpectrumBandsConverter : IValueConverter
    {
        public static readonly Int32Collection Empty = new Int32Collection();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var option = value as SelectionConfigurationOption;
            if (option == null)
            {
                return Empty;
            }
            return new Int32Collection(SpectrumBehaviourConfiguration.GetBands(option));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
