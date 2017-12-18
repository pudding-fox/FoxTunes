using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
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

        public static void Enumerate<T>(this IEnumerable<T> sequence)
        {
            foreach (var element in sequence) ;
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

        public static IDbCommand CreateCommand(this IDbConnection connection, string commandText)
        {
            var parameters = default(IDbParameterCollection);
            return connection.CreateCommand(commandText, Enumerable.Empty<string>(), out parameters);
        }

        public static IDbCommand CreateCommand(this IDbConnection connection, string commandText, IEnumerable<string> parameterNames, out IDbParameterCollection parameters)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text is required.");
            }
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            if (parameterNames != null)
            {
                foreach (var parameterName in parameterNames)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = parameterName;
                    command.Parameters.Add(parameter);
                }
            }
            parameters = new DbParameterCollection(command.Parameters);
            return command;
        }

        public static T GetValue<T>(this IDataRecord dataRecord, int index)
        {
            var value = dataRecord.GetValue(index);
            if (DBNull.Value.Equals(value))
            {
                return default(T);
            }
            if (typeof(T).IsEnum)
            {
                return (T)Enum.ToObject(typeof(T), value);
            }
            return (T)Convert.ChangeType(value, typeof(T));
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

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type NullableType(this Type type)
        {
            if (!type.IsNullable())
            {
                throw new ArgumentException("Type \"{0}\" is not Nullable.", type.Name);
            }
            return type.GetGenericArguments().First();
        }

        private class DbParameterCollection : IDbParameterCollection
        {
            public DbParameterCollection(IDataParameterCollection parameters)
            {
                this.Parameters = parameters;
            }

            public IDataParameterCollection Parameters { get; private set; }

            public int Count
            {
                get
                {
                    return this.Parameters.Count;
                }
            }

            public bool Contains(string name)
            {
                return this.Parameters.Contains(name);
            }

            public object this[string name]
            {
                get
                {
                    return (this.Parameters[name] as IDataParameter).Value;
                }
                set
                {
                    (this.Parameters[name] as IDataParameter).Value = value;
                }
            }
        }
    }
}
