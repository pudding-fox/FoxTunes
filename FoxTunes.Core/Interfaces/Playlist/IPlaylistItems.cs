using System.Collections.Generic;
using System.Collections.Specialized;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistItems : IBaseComponent, ICollection<IPlaylistItem>, INotifyCollectionChanged
    {
        IPlaylist Playlist { get; }

        IPlaylistItem Create(string fileName);

        int IndexOf(IPlaylistItem item);

        IPlaylistItem this[int index] { get; set; }
    }
}
