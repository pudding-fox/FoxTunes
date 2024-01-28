using System;

namespace FoxTunes.Interfaces
{
    public interface IBassOutput : IOutput, IConfigurableComponent, IDisposable
    {
        int Rate { get; }

        bool EnforceRate { get; }

        bool Float { get; }

        IBassStreamFactory StreamFactory { get; }

        IBassStreamPipeline Pipeline { get; }

        void FreeStream(int channelHandle);

        event EventHandler Init;

        event EventHandler Free;
    }
}
