using System;

namespace FoxTunes.Interfaces
{
    public interface IDatabase : IStandardComponent, IDisposable
    {
        void Save<T>(T value);

        void Load<T>(T value);
    }
}
