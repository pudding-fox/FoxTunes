using System;
using System.Globalization;

namespace FoxTunes
{
    public static class DateTimeHelper
    {
        public static string ToString(DateTime value)
        {
            if (value.TimeOfDay == TimeSpan.Zero)
            {
                return value.ToString(Constants.DATE_FORMAT, CultureInfo.InvariantCulture);
            }
            else
            {
                return value.ToString(Constants.DATE_TIME_FORMAT, CultureInfo.InvariantCulture);
            }
        }

        public static DateTime FromString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default(DateTime);
            }
            var date = default(DateTime);
            if (DateTime.TryParseExact(value, Constants.DATE_TIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
            {
                return date;
            }
            if (DateTime.TryParseExact(value, Constants.DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
            {
                return date;
            }
            return default(DateTime);
        }
    }
}
