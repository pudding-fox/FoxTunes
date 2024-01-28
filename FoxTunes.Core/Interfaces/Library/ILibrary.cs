using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface ILibrary : IBaseComponent
    {
        ObservableCollection<LibraryItem> Items { get; }
    }
}
