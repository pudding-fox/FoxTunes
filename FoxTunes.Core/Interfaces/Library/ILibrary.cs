namespace FoxTunes.Interfaces
{
    public interface ILibrary : IBaseComponent
    {
        IDatabaseQuery<LibraryItem> Query { get; }

        IDatabaseSet<LibraryItem> Set { get; }
    }
}
