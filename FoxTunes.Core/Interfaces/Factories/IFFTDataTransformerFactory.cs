namespace FoxTunes.Interfaces
{
    public interface IFFTDataTransformerFactory : IStandardComponent
    {
        IFFTDataTransformer Create(int[] bands);
    }
}
