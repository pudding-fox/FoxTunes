using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace FoxTunes
{
    public class RecordPersister<T> : BaseComponent where T : IPersistableComponent
    {
        public RecordPersister(ICore core, IDatabase database, RecordPersisterMode mode, IDbTransaction transaction = null)
        {
            this.Core = core;
            this.Database = database;
            this.Mode = mode;
            this.Transaction = transaction;
        }

        public ICore Core { get; private set; }

        public IDatabase Database { get; private set; }

        public RecordPersisterMode Mode { get; private set; }

        public IDbTransaction Transaction { get; private set; }

        public void Persist(T item)
        {
            this.Persist(new[] { item });
        }

        public void Persist(IEnumerable<T> items)
        {
            switch (this.Mode)
            {
                case RecordPersisterMode.AddOrUpdate:
                    this.Add(items.Where(item => item.Id == default(int)));
                    this.Update(items.Where(item => item.Id != default(int)));
                    break;
                case RecordPersisterMode.Delete:
                    this.Delete(items);
                    break;
            }
        }

        protected virtual void Add(IEnumerable<T> items)
        {
            this.Execute(items, this.Database.Queries.Insert<T>(), (item, command) => item.Id = Convert.ToInt32(command.ExecuteScalar()));
        }

        protected virtual void Update(IEnumerable<T> items)
        {
            this.Execute(items, this.Database.Queries.Update<T>(), (item, command) => command.ExecuteNonQuery());
        }

        protected virtual void Delete(IEnumerable<T> items)
        {
            this.Execute(items, this.Database.Queries.Delete<T>(), (item, command) => command.ExecuteNonQuery());
        }

        protected virtual void Execute(IEnumerable<T> items, IDatabaseQuery query, Action<T, IDbCommand> action)
        {
            var parameters = default(IDbParameterCollection);
            using (var command = this.Database.CreateCommand(query, out parameters, this.Transaction))
            {
                foreach (var item in items)
                {
                    ParameterPopulator.Populate(query, parameters, item);
                    action(item, command);
                }
            }
        }

        public static class ParameterPopulator
        {
            const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;

            private static IEnumerable<Func<string, PropertyInfo>> PropertyResolutionStrategies = new Func<string, PropertyInfo>[]
            {
                name => typeof(T).GetProperty(name, BINDING_FLAGS),
                name => typeof(T).GetProperty(name.Replace("_", ""), BINDING_FLAGS)
            };

            private static readonly ParameterPopulatorHandler Handler = CreateHandler();

            private delegate void ParameterPopulatorHandler(IDatabaseQuery query, IDbParameterCollection parameters, T item);

            public static void Populate(IDatabaseQuery query, IDbParameterCollection parameters, T item)
            {
                Handler(query, parameters, item);
            }

            private static ParameterPopulatorHandler CreateHandler()
            {
                //TODO: Build expression tree.
                return (query, parameters, item) =>
                {
                    foreach (var name in query.ParameterNames)
                    {
                        var property = GetProperty(name);
                        var value = GetValue(item, property);
                        parameters[name] = value;
                    }
                };
            }

            private static PropertyInfo GetProperty(string name)
            {
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

            private static object GetValue(T item, PropertyInfo property)
            {
                return property.GetValue(item);
            }
        }
    }

    public enum RecordPersisterMode : byte
    {
        None = 0,
        AddOrUpdate = 1,
        Delete = 2
    }
}
