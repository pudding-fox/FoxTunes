using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Windows;

namespace FoxTunes
{
    public interface IUIPlaylistColumnProvider : IBaseComponent
    {
        string Id { get; }

        string Name { get; }

        string Description { get; }

        DataTemplate CellTemplate { get; }

        IEnumerable<string> MetaData { get; }
    }
}
