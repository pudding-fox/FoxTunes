using System;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseFactory : IStandardFactory
    {
        DatabaseFactoryFlags Flags { get; }

        bool Test();

        void Initialize();

        IDatabaseComponent Create();
    }

    [Flags]
    public enum DatabaseFactoryFlags : byte
    {
        None = 0,
        ConfirmCreate = 1
    }
}
