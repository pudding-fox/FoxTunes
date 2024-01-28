using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface IPlaylist : IBaseComponent
    {
        IPersistableSet<PlaylistItem> Set { get; }

        ObservableCollection<PlaylistItem> Items { get; }
    }
}
