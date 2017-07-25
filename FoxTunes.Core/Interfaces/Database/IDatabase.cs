using System;

namespace FoxTunes.Interfaces
{
    public interface IDatabase : IStandardComponent, IDisposable
    {
        IDatabaseSet<T> GetSet<T>() where T : class;

        IDatabaseQuery<T> GetQuery<T>() where T : class;

        int SaveChanges();
    }
}
