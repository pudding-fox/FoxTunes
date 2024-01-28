using FoxTunes.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace FoxTunes
{
    public class RecordEnumerator<T> : BaseComponent, ICountable, IEnumerable<T> where T : IBaseComponent
    {
        public RecordEnumerator(ICore core, IDatabase database, IDatabaseQuery query, IDbTransaction transaction = null) : this(core, database, query, null, transaction)
        {

        }

        public RecordEnumerator(ICore core, IDatabase database, IDatabaseQuery query, Action<IDbParameterCollection> parameters, IDbTransaction transaction = null)
        {
            this.Core = core;
            this.Database = database;
            this.Query = query;
            this.Parameters = parameters;
            this.Transaction = transaction;
        }

        public ICore Core { get; private set; }

        public IDatabase Database { get; private set; }

        public IDatabaseQuery Query { get; private set; }

        public Action<IDbParameterCollection> Parameters { get; private set; }

        public IDbTransaction Transaction { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            using (var reader = this.Database.CreateReader(this.Query, this.Parameters, this.Transaction))
            {
                while (reader.Read())
                {
                    yield return RecordFactory.Create(this.Core, reader);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                var count = 0;
                using (var reader = this.Database.CreateReader(this.Query, this.Parameters, this.Transaction))
                {
                    while (reader.Read())
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public static class RecordFactory
        {
            const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;

            private static IEnumerable<Func<string, PropertyInfo>> PropertyResolutionStrategies = new Func<string, PropertyInfo>[]
            {
                name => typeof(T).GetProperty(name, BINDING_FLAGS),
                name => typeof(T).GetProperty(name.Replace("_", ""), BINDING_FLAGS)
            };

            private static readonly RecordFactoryHandler Handler = CreateHandler();

            private delegate T RecordFactoryHandler(IDataReader reader);

            public static T Create(ICore core, IDataReader reader)
            {
                var component = Handler(reader);
                component.InitializeComponent(core);
                return component;
            }

            private static RecordFactoryHandler CreateHandler()
            {
                //TODO: Build expression tree.
                return reader =>
                {
                    var record = Activator.CreateInstance<T>();
                    for (var a = 0; a < reader.FieldCount; a++)
                    {
                        if (reader.IsDBNull(a))
                        {
                            continue;
                        }
                        var property = GetProperty(reader, a);
                        var value = GetValue(reader, a, property);
                        property.SetValue(record, value);
                    }
                    return record;
                };
            }

            private static PropertyInfo GetProperty(IDataReader reader, int field)
            {
                var name = reader.GetName(field);
                foreach (var strategy in PropertyResolutionStrategies)
                {
                    var property = strategy(name);
                    if (property != null)
                    {
                        return property;
                    }
                }
                throw new InvalidOperationException(string.Format("Failed to locate property of type \"{0}\" for database column \"{1}\".", typeof(T).Name, name));
            }

            private static object GetValue(IDataReader reader, int field, PropertyInfo property)
            {
                if (property.PropertyType.IsEnum)
                {
                    return Enum.ToObject(property.PropertyType, reader.GetValue(field));
                }
                else if (property.PropertyType.IsNullable())
                {
                    return Convert.ChangeType(reader.GetValue(field), property.PropertyType.NullableType());
                }
                else
                {
                    return Convert.ChangeType(reader.GetValue(field), property.PropertyType);
                }
            }
        }
    }
}
