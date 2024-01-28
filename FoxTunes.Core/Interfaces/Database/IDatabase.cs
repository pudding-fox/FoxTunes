using System;

namespace FoxTunes.Interfaces
{
    public interface IDatabase : IStandardComponent, IDisposable
    {
        IPersistableSet GetSet(Type type);

        IPersistableSet<T> GetSet<T>() where T : class;
    }
}
