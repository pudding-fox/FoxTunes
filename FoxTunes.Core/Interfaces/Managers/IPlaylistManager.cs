using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistManager : IStandardManager, IBackgroundTaskSource
    {
        Task Add(int sequence, IEnumerable<string> paths);

        Task Add(int sequence, IEnumerable<LibraryItem> libraryItems);

        event EventHandler Updated;

        Task Play(PlaylistItem playlistItem);

        Task Next();

        Task Previous();

        Task Clear();

        PlaylistItem CurrentItem { get; }

        event EventHandler CurrentItemChanged;
    }
}
