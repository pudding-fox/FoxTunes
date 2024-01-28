using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistBrowser : IStandardComponent
    {
        IEnumerable<PlaylistItem> GetItems();

        Task<PlaylistItem> Get(int sequence);

        Task<PlaylistItem> Get(string fileName);

        Task<PlaylistItem> GetNext(bool navigate);

        Task<PlaylistItem> GetPrevious(bool navigate);

        Task<int> GetInsertIndex();
    }
}
