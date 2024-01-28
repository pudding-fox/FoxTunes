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
}
