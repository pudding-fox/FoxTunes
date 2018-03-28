namespace FoxTunes.Interfaces
{
    public interface IBassStreamPipelineFactory
    {
        IBassStreamPipeline CreatePipeline(bool dsd, int rate, int channels);
    }
}
