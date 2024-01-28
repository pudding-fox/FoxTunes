using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistManager : IStandardManager, IBackgroundTaskSource
    {
        Task Add(IEnumerable<string> paths);

        Task Insert(int index, IEnumerable<string> paths);

        Task Add(IEnumerable<LibraryItem> libraryItems);

        Task Insert(int index, IEnumerable<LibraryItem> libraryItems);

        event EventHandler Updated;

        Task Play(PlaylistItem playlistItem);

        bool CanNavigate { get; }

        Task Next();

        Task Previous();

        Task Clear();

        PlaylistItem CurrentItem { get; }

        event EventHandler CurrentItemChanged;
    }
}
