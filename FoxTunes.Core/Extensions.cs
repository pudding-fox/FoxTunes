using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        public static IEnumerable<T> Enumerate<T>(this IEnumerable<T> sequence)
        {
            foreach (var element in sequence) ;
            return sequence;
        }

        public static bool Contains(this IEnumerable<string> sequence, string value, bool ignoreCase)
        {
            if (!ignoreCase)
            {
                return sequence.Contains(value);
            }
            return sequence.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool Contains(this string subject, string value, bool ignoreCase)
        {
            if (!ignoreCase)
            {
                return subject.Contains(value);
            }
            return subject.IndexOf(value, StringComparison.OrdinalIgnoreCase) != -1;
        }

        public static string IfNullOrEmpty(this string value, string alternative)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            return alternative;
        }

        public static int IndexOf<T>(this IEnumerable<T> sequence, T element)
        {
            var index = 0;
            var enumerator = sequence.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != null && enumerator.Current.Equals(element))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static string GetExtension(this string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension) || extension.Length <= 1)
            {
                return string.Empty;
            }
            return extension.Substring(1).ToLower(CultureInfo.InvariantCulture);
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            return sequence.Do(action).Enumerate();
        }

        public static IEnumerable<T> Try<T>(this IEnumerable<T> sequence, Action<T> action, Action<T, Exception> errorHandler = null)
        {
            return sequence.ForEach(element => element.Try(action, errorHandler));
        }

        public static T Try<T>(this T value, Action<T> action, Action<T, Exception> errorHandler = null)
        {
            try
            {
                action(value);
            }
            catch (Exception e)
            {
                if (errorHandler != null)
                {
                    errorHandler(value, e);
                }
            }
            return value;
        }
    }
}
