using System;
using System.Data;

namespace FoxTunes.Interfaces
{
    public interface IDatabase : IStandardComponent
    {
        IDatabaseSets Sets { get; }

        IDatabaseQueries Queries { get; }

        IDbTransaction BeginTransaction();

        IDbCommand CreateCommand(IDatabaseQuery query, IDbTransaction transaction = null);

        IDbCommand CreateCommand(IDatabaseQuery query, out IDbParameterCollection parameters, IDbTransaction transaction = null);

        IDataReader CreateReader(IDatabaseQuery query, IDbTransaction transaction = null);

        IDataReader CreateReader(IDatabaseQuery query, Action<IDbParameterCollection> parameters, IDbTransaction transaction = null);

        T ExecuteCommand<T>(IDatabaseQuery query, IDbTransaction transaction = null);

        T ExecuteCommand<T>(IDatabaseQuery query, Action<IDbParameterCollection> parameters, IDbTransaction transaction = null);

        IDatabaseSet<T> GetSet<T>(IDbTransaction transaction = null) where T : IPersistableComponent;
    }
}
