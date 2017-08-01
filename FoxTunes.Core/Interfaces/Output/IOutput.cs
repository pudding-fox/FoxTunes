using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IOutput : IStandardComponent
    {
        bool IsSupported(string fileName);

        Task<IOutputStream> Load(string fileName);

        Task Unload(IOutputStream stream);
    }
}
