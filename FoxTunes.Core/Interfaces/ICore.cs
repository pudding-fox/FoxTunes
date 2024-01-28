using System;

namespace FoxTunes.Interfaces
{
    public interface ICore : IDisposable
    {
        IStandardComponents Components { get; }

        IStandardManagers Managers { get; }

        IStandardFactories Factories { get; }

        IFileAssociations Associations { get; }

        void Load();
    }
}
