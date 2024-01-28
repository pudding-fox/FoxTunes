using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistCache : IStandardComponent
    {
        IEnumerable<PlaylistItem> GetItems(Func<IEnumerable<PlaylistItem>> factory);
    }
}
