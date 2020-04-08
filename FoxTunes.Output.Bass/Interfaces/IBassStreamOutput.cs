namespace FoxTunes.Interfaces
{
    public interface IBassStreamOutput : IBassStreamControllable, IBassStreamComponent
    {
        bool IsPlaying { get; }

        bool IsPaused { get; }

        bool IsStopped { get; }

        int Latency { get; }

        bool CanControlVolume { get; }

        float Volume { get; set; }

        bool CheckFormat(int rate, int channels);

        int GetData(float[] buffer);
    }
}
