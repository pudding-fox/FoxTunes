using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistCache : IStandardComponent
    {
        bool TryGetItemById(int id, out PlaylistItem playlistItem);

        bool TryGetItemsByLibraryId(int id, out IEnumerable<PlaylistItem> playlistItems);

        PlaylistItem[] GetItems(Func<IEnumerable<PlaylistItem>> factory);
    }
}
