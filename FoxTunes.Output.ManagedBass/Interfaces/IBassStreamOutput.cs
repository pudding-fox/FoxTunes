using System;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamOutput : IBassStreamControllable, IBassStreamComponent
    {
        bool IsPlaying { get; }

        bool IsPaused { get; }

        bool IsStopped { get; }

        int Latency { get; }

        bool CheckFormat(int rate, int channels);
    }
}
