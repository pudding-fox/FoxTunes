using System;

namespace FoxTunes
{
    public class DateHelper
    {
        public static string toLocaleDateString(string value)
        {
            var date = DateTimeHelper.FromString(value);
            if (date == default(DateTime))
            {
                return null;
            }
            return date.ToShortDateString() + " " + date.ToShortTimeString();
        }
    }

    public class NumberHelper
    {
        public static float parseFloat(string value)
        {
            var parsed = default(float);
            if (float.TryParse(value, out parsed))
            {
                return parsed;
            }
            return 0.0f;
        }

        public static string toFixed(float value, int digits)
        {
            return value.ToString(string.Format("N{0}", digits));
        }
    }
}
