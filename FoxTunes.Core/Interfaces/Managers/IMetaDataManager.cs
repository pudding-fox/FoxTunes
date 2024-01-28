using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataManager : IStandardManager, IBackgroundTaskSource, IReportSource
    {
        Task Rescan(IEnumerable<LibraryItem> libraryItems);

        Task Rescan(IEnumerable<PlaylistItem> playlistItems);

        Task Save(IEnumerable<LibraryItem> libraryItems, bool write, params string[] names);

        Task Save(IEnumerable<PlaylistItem> playlistItems, bool write, params string[] names);
    }
}
