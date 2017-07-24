using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface ILibrary : IBaseComponent
    {
        IPersistableSet<LibraryItem> Set { get; }

        ObservableCollection<LibraryItem> Items { get; }
    }
}
