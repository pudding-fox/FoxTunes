using System;
namespace FoxTunes.Interfaces
{
    public interface IPlaylist : IBaseComponent
    {
        IPlaylistItems Items { get; }

        IPlaylistItem SelectedItem { get; set; }

        event EventHandler SelectedItemChanging;

        event EventHandler SelectedItemChanged;
    }
}
