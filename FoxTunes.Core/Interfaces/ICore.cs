using FoxDb.Interfaces;
using System;

namespace FoxTunes.Interfaces
{
    public interface ICore : IDisposable
    {
        ICoreSetup Setup { get; }

        IStandardComponents Components { get; }

        IStandardManagers Managers { get; }

        IStandardFactories Factories { get; }

        void Load();

        void Initialize();

        void CreateDefaultData(IDatabase database);
    }
}
