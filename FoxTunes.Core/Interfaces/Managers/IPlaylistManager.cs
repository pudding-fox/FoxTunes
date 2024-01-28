using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistManager : IStandardManager, IBackgroundTaskSource, IInvocableComponent, IDatabaseInitializer
    {
        Task Add(IEnumerable<string> paths, bool clear);

        Task Insert(int index, IEnumerable<string> paths, bool clear);

        Task Add(LibraryHierarchyNode libraryHierarchyNode, bool clear);

        Task Insert(int index, LibraryHierarchyNode libraryHierarchyNode, bool clear);

        Task Add(IEnumerable<LibraryHierarchyNode> libraryHierarchyNodes, bool clear);

        Task Insert(int index, IEnumerable<LibraryHierarchyNode> libraryHierarchyNodes, bool clear);

        Task Move(IEnumerable<PlaylistItem> playlistItems);

        Task Move(int index, IEnumerable<PlaylistItem> playlistItems);

        Task Remove(IEnumerable<PlaylistItem> playlistItems);

        Task Crop(IEnumerable<PlaylistItem> playlistItems);

        Task Play(PlaylistItem playlistItem);

        Task Play(string fileName);

        Task Play(int sequence);

        bool CanNavigate { get; }

        event EventHandler CanNavigateChanged;

        Task Next();

        Task Previous();

        Task Clear();

        PlaylistItem CurrentItem { get; }

        event AsyncEventHandler CurrentItemChanged;

        PlaylistItem[] SelectedItems { get; set; }

        event EventHandler SelectedItemsChanged;

        Task SetRating(IEnumerable<PlaylistItem> playlistItems, byte rating);

        Task IncrementPlayCount(PlaylistItem playlistItem);
    }
}
