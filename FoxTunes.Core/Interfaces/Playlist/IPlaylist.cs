using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface IPlaylist : IBaseComponent
    {
        ObservableCollection<PlaylistItem> Items { get; }
    }
}
