using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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

        public static bool HasCustomAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            var attribute = default(T);
            return type.HasCustomAttribute<T>(out attribute);
        }

        public static bool HasCustomAttribute<T>(this Type type, out T attribute) where T : Attribute
        {
            return type.HasCustomAttribute<T>(false, out attribute);
        }

        public static bool HasCustomAttribute<T>(this Type type, bool inherit, out T attribute) where T : Attribute
        {
            if (!type.Assembly.ReflectionOnly)
            {
                return (attribute = type.GetCustomAttribute<T>(inherit)) != null;
            }
            var sequence = type.GetCustomAttributesData();
            foreach (var element in sequence)
            {
                if (!string.Equals(element.Constructor.DeclaringType.FullName, typeof(T).FullName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                attribute = AssemblyRegistry.Instance.GetExecutableType(type).GetCustomAttribute<T>();
                return true;
            }
            attribute = default(T);
            return false;
        }

        public static T GetCustomAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes<T>(inherit).FirstOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }

        public static T Do<T>(this T value, Action<T> action)
        {
            action(value);
            return value;
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

        public static string GetName(this string fileName)
        {
            if (string.IsNullOrEmpty(Path.GetPathRoot(fileName)))
            {
                return fileName;
            }
            var name = Path.GetFileName(fileName);
            return name;
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

        public static IEnumerable<string> GetLines(this string sequence)
        {
            if (string.IsNullOrEmpty(sequence))
            {
                yield break;
            }
            foreach (var element in sequence.Split('\n'))
            {
                yield return element.TrimEnd('\r');
            }
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

        public static IEnumerable<T> AddRange<T>(this IList<T> list, IEnumerable<T> sequence)
        {
            foreach (var element in sequence)
            {
                list.Add(element);
            }
            return sequence;
        }

        public static IEnumerable<T> RemoveRange<T>(this IList<T> list, IEnumerable<T> sequence)
        {
            foreach (var element in sequence)
            {
                list.Remove(element);
            }
            return sequence;
        }

        public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            var value = default(TValue);
            return dictionary.TryRemove(key, out value);
        }
    }
}
