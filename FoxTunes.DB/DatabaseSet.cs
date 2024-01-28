using FoxTunes.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FoxTunes
{
    public class DatabaseSet<T> : IDatabaseSet<T> where T : IPersistableComponent
    {
        public DatabaseSet(ICore core, IDatabase database, IDbTransaction transaction = null)
        {
            this.Core = core;
            this.Database = database;
            this.Transaction = transaction;
        }

        public ICore Core { get; private set; }

        public IDatabase Database { get; private set; }

        public IDbTransaction Transaction { get; private set; }

        public int Count
        {
            get
            {
                return this.Database.ExecuteCommand<int>(this.Database.Queries.Count<T>(), this.Transaction);
            }
        }

        public T Find(object id)
        {
            using (var reader = this.Database.CreateReader(this.Database.Queries.Find<T>(), parameters => parameters["id"] = id, this.Transaction))
            {
                if (!reader.Read())
                {
                    return default(T);
                }
                return RecordEnumerator<T>.RecordFactory.Create(this.Core, reader);
            }
        }

        public IEnumerable<T> Query(IDatabaseQuery query)
        {
            return this.Query(query, parameters =>
            {
                if (parameters.Count > 0)
                {
                    throw new InvalidOperationException("Query contains parameters, use Query(query, out parameters).");
                }
            });
        }

        public IEnumerable<T> Query(IDatabaseQuery query, Action<IDbParameterCollection> parameters)
        {
            return new RecordEnumerator<T>(this.Core, this.Database, query, parameters, this.Transaction);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new RecordEnumerator<T>(this.Core, this.Database, this.Database.Queries.Select<T>(), this.Transaction).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void AddOrUpdate(T item)
        {
            this.AddOrUpdate(new[] { item });
        }

        public void AddOrUpdate(IEnumerable<T> items)
        {
            var persister = new RecordPersister<T>(this.Core, this.Database, RecordPersisterMode.AddOrUpdate, this.Transaction);
            persister.Persist(items);
        }

        public void Delete(T item)
        {
            this.Delete(new[] { item });
        }

        public void Delete(IEnumerable<T> items)
        {
            var persister = new RecordPersister<T>(this.Core, this.Database, RecordPersisterMode.Delete, this.Transaction);
            persister.Persist(items);
        }

        public void Clear()
        {
            this.Delete(this);
        }
    }
}
