using System;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamOutput : IBassStreamControllable, IBassStreamComponent
    {
        bool IsPlaying { get; }

        bool IsPaused { get; }

        bool IsStopped { get; }

        int Latency { get; }

        BassStreamOutputCapability Capabilities { get; }

        bool CheckFormat(int rate, int channels);
    }

    [Flags]
    public enum BassStreamOutputCapability : byte
    {
        None = 0,
        DSD = 1
    }
}
