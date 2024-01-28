using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistCache : IStandardComponent
    {
        bool Contains(Func<PlaylistItem, bool> predicate);

        IEnumerable<PlaylistItem> GetItems(Func<IEnumerable<PlaylistItem>> factory);
    }
}
