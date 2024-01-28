using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IHierarchyManager : IStandardManager, IBackgroundTaskSource
    {
        Task Build(bool reset);

        Task Clear();
    }
}
