namespace FoxTunes.Interfaces
{
    public interface IBassStreamControllable
    {
        void PreviewPlay(IBassStreamPipeline pipeline);

        void PreviewPause(IBassStreamPipeline pipeline);

        void PreviewResume(IBassStreamPipeline pipeline);

        void PreviewStop(IBassStreamPipeline pipeline);

        void Play();

        void Pause();

        void Resume();

        void Stop();
    }
}
