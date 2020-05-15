using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IFileActionHandler : IBaseComponent
    {
        bool CanHandle(string path);

        Task Handle(IEnumerable<string> paths);
    }
}
