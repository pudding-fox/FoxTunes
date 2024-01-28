using FoxTunes.Interfaces;
using System.Windows;

namespace FoxTunes
{
    public interface IUIPlaylistColumnProvider : IBaseComponent
    {
        DataTemplate CellTemplate { get; }
    }
}
