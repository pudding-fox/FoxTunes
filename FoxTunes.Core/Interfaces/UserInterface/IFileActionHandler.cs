using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IFileActionHandler : IBaseComponent
    {
        Task<bool> Handle(string fileName);
    }
}
