using FoxDb.Interfaces;
using System;

namespace FoxTunes.Interfaces
{
    public interface ICore : IDisposable
    {
        IStandardComponents Components { get; }

        IStandardManagers Managers { get; }

        IStandardFactories Factories { get; }

        void Load();

        void Initialize();

        void CreateDefaultData(IDatabase database);

        CoreFlags Flags { get; }
    }

    [Flags]
    public enum CoreFlags : byte
    {
        None = 0,
        Headless = 1
    }
}
