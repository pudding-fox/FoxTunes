using System;

namespace FoxTunes.Interfaces
{
    public interface IBassOutput : IOutput, IConfigurableComponent, IDisposable
    {
        int Rate { get; }

        bool EnforceRate { get; }

        bool Float { get; }

        bool PlayFromMemory { get; }

        IBassStreamFactory StreamFactory { get; }

        void WithPipeline(Action<IBassStreamPipeline> pipeline);

        void WithPipeline(BassOutputStream stream, Action<IBassStreamPipeline> pipeline);

        event EventHandler Init;

        event EventHandler Free;
    }
}
