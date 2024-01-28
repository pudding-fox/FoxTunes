namespace FoxTunes.Interfaces
{
    public interface ILibraryBrowser : IStandardComponent
    {
        LibraryItem Get(int id);
    }
}
