using ManagedBass;
using System;

namespace FoxTunes.Interfaces
{
    public interface IBassOutput : IConfigurableComponent, IDisposable
    {
        int Rate { get; }

        bool EnforceRate { get; }

        bool Float { get; }

        BassOutputMode Mode { get; }

        int DirectSoundDevice { get; }

        int AsioDevice { get; }

        bool DsdDirect { get; }

        bool Resampler { get; }

        BassFlags Flags { get; }

        void FreeStream(int channelHandle);
    }
}
