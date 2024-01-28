using System;

namespace FoxTunes.Interfaces
{
    public interface IBassOutput : IOutput, IConfigurableComponent, IDisposable
    {
        int Rate { get; }

        bool EnforceRate { get; }

        bool Float { get; }

        bool PlayFromMemory { get; }

        event EventHandler Init;

        event EventHandler Free;
    }
}
