using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface ILibraryManager : IStandardManager, IBackgroundTaskSource
    {
        Task Add(IEnumerable<string> paths);

        Task Clear();
    }
}
