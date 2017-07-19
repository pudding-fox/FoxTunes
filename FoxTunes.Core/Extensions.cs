using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static partial class Extensions
    {
        public static T Value<T>(this IEnumerable<INamedValue> items, string name)
        {
            var item = items.FirstOrDefault(
                _item => string.Equals(_item.Name, name, StringComparison.InvariantCultureIgnoreCase)
            );
            if (item == null)
            {
                return default(T);
            }
            return (T)Convert.ChangeType(item.Value, typeof(T));
        }
    }
}
