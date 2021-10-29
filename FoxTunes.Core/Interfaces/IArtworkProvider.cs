using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IArtworkProvider : IStandardComponent
    {
        string Find(string path, ArtworkType type);

        Task<string> Find(IFileData fileData, ArtworkType type);
    }
}
