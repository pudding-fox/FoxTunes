namespace FoxTunes.Interfaces
{
    public interface IBassEncoderOutputPath : IBaseComponent
    {
        string GetDirectoryName(IFileData fileData);
    }
}
