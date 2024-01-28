using FoxTunes.Interfaces;
using System;
using System.Data;

namespace FoxTunes
{
    public abstract class Database : StandardComponent, IDatabase
    {
        public ICore Core { get; private set; }

        public IDatabaseSets Sets { get; private set; }

        public abstract IDatabaseQueries Queries { get; protected set; }

        public abstract IDbConnection Connection { get; protected set; }

        public IDbTransaction BeginTransaction()
        {
            Logger.Write(this, LogLevel.Debug, "Beginning transaction.");
            return this.Connection.BeginTransaction();
        }

        public IDbCommand CreateCommand(IDatabaseQuery query, IDbTransaction transaction = null)
        {
            var parameters = default(IDbParameterCollection);
            try
            {
                return this.CreateCommand(query, out parameters, transaction);
            }
            finally
            {
                if (parameters.Count > 0)
                {
                    throw new InvalidOperationException("Query contains parameters, use CreateCommand(query, out parameters).");
                }
            }
        }

        public IDbCommand CreateCommand(IDatabaseQuery query, out IDbParameterCollection parameters, IDbTransaction transaction = null)
        {
            var command = this.Connection.CreateCommand(query.CommandText, query.ParameterNames, out parameters);
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            return command;
        }

        public IDataReader CreateReader(IDatabaseQuery query, IDbTransaction transaction = null)
        {
            var command = this.CreateCommand(query, transaction);
            return command.ExecuteReader();
        }

        public IDataReader CreateReader(IDatabaseQuery query, Action<IDbParameterCollection> action, IDbTransaction transaction = null)
        {
            var parameters = default(IDbParameterCollection);
            var command = this.CreateCommand(query, out parameters, transaction);
            if (action != null)
            {
                action(parameters);
            }
            return command.ExecuteReader();
        }

        public T ExecuteCommand<T>(IDatabaseQuery query, IDbTransaction transaction = null)
        {
            var command = this.CreateCommand(query, transaction);
            return (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T));
        }

        public T ExecuteCommand<T>(IDatabaseQuery query, Action<IDbParameterCollection> action, IDbTransaction transaction = null)
        {
            var parameters = default(IDbParameterCollection);
            var command = this.CreateCommand(query, out parameters, transaction);
            action(parameters);
            return (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T));
        }

        public IDatabaseSet<T> GetSet<T>(IDbTransaction transaction = null) where T : IPersistableComponent
        {
            return new DatabaseSet<T>(this.Core, this, transaction);
        }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Sets = new DatabaseSets();
            this.Sets.InitializeComponent(core);
            base.InitializeComponent(core);
        }
    }
}