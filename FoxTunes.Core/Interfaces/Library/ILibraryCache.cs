using System;

namespace FoxTunes.Interfaces
{
    public interface ILibraryCache : IStandardComponent
    {
        bool TryGetItem(int id, out LibraryItem playlistItem);

        LibraryItem GetItem(int id, Func<LibraryItem> factory);
    }
}
