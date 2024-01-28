namespace FoxTunes.Interfaces
{
    public interface ILibraryItemFactory : IStandardFactory
    {
        LibraryItem Create(string fileName);
    }
}
