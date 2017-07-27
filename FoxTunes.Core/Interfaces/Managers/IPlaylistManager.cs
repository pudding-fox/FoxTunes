using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistManager : IStandardManager, IBackgroundTaskSource
    {
        void Add(IEnumerable<string> paths);

        void Add(IEnumerable<LibraryItem> libraryItems);

        event EventHandler Updated;

        void Next();

        void Previous();

        void Clear();

        PlaylistItem CurrentItem { get; }

        event EventHandler CurrentItemChanged;
    }
}
