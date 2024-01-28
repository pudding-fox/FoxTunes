using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistBrowser : IStandardComponent
    {
        PlaylistBrowserState State { get; }

        event EventHandler StateChanged;

        PlaylistItem[] GetItems();

        Task<PlaylistItem> Get(int sequence);

        Task<PlaylistItem> Get(string fileName);

        Task<PlaylistItem> GetNext(bool navigate);

        Task<PlaylistItem> GetPrevious(bool navigate);

        Task<int> GetInsertIndex();
    }

    public enum PlaylistBrowserState : byte
    {
        None,
        Loading
    }
}
