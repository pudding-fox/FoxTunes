using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistManager : IStandardManager, IBackgroundTaskSource, IInvocableComponent
    {
        Task Add(IEnumerable<string> paths);

        Task Insert(int index, IEnumerable<string> paths);

        Task Add(LibraryHierarchyNode libraryHierarchyNode);

        Task Insert(int index, LibraryHierarchyNode libraryHierarchyNode);

        Task Remove(IEnumerable<PlaylistItem> playlistItems);

        Task Crop(IEnumerable<PlaylistItem> playlistItems);

        Task Play(PlaylistItem playlistItem);

        bool CanNavigate { get; }

        PlaylistItem GetNext();

        PlaylistItem GetPrevious();

        int GetInsertIndex();

        Task Next();

        Task Previous();

        Task Clear();

        PlaylistItem CurrentItem { get; }

        event EventHandler CurrentItemChanged;
    }
}
