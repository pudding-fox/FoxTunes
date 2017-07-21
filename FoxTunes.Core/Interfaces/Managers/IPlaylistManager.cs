using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistManager : IStandardManager
    {
        void Add(IEnumerable<string> paths);

        void Next();

        void Previous();

        void Clear();

        PlaylistItem CurrentItem { get; }

        ObservableCollection<PlaylistItem> Items { get; }
    }
}
