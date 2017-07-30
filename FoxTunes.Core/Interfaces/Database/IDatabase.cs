using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FoxTunes.Interfaces
{
    public interface IDatabase : IStandardComponent, IDisposable
    {
        IDatabaseSet<T> GetSet<T>() where T : class;

        IDatabaseQuery<T> GetQuery<T>() where T : class;

        IDatabaseQuery<TMember> GetMemberQuery<T, TMember>(T item, Expression<Func<T, TMember>> member)
            where T : class
            where TMember : class;

        IDatabaseQuery<TMember> GetMemberQuery<T, TMember>(T item, Expression<Func<T, ICollection<TMember>>> member)
            where T : class
            where TMember : class;

        void Interlocked(Action action);

        T Interlocked<T>(Func<T> func);

        void Interlocked(Action action, TimeSpan timeout);

        T Interlocked<T>(Func<T> func, TimeSpan timeout);

        void WithAutoDetectChanges(Action action);

        int SaveChanges();
    }
}
