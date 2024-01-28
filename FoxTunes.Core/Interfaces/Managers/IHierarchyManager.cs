using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IHierarchyManager : IStandardManager, IBackgroundTaskSource
    {
        Task AddHierarchy(LibraryHierarchy libraryHierarchy);

        Task DeleteHierarchy(LibraryHierarchy libraryHierarchy);

        Task BuildHierarchies();
    }
}
