using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistManager : IStandardManager
    {
        void AddDirectory(string directoryName);

        void AddFiles(params string[] fileName);

        void Next();

        void Previous();

        void Clear();

        PlaylistItem CurrentItem { get; }

        ObservableCollection<PlaylistItem> Items { get; }
    }
}
