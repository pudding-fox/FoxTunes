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

        public static T GetCustomAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes<T>(inherit).FirstOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> sequence, Action action)
        {
            foreach (var element in sequence)
            {
                action();
                yield return element;
            }
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (var element in sequence)
            {
                action(element);
                yield return element;
            }
        }
    }
}
