using System;
using System.Collections.ObjectModel;
namespace FoxTunes.Interfaces
{
    public interface IPlaylist : IBaseComponent
    {
        ObservableCollection<IPlaylistItem> Items { get; }

        IPlaylistItem SelectedItem { get; set; }

        event EventHandler SelectedItemChanging;

        event EventHandler SelectedItemChanged;
    }
}
