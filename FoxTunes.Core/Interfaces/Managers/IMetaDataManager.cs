using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataManager : IStandardManager, IBackgroundTaskSource
    {
        Task Save(IEnumerable<LibraryItem> libraryItems);

        Task Save(IEnumerable<PlaylistItem> playlistItems);
    }
}
