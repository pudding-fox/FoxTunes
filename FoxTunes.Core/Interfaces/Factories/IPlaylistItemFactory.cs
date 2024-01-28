namespace FoxTunes.Interfaces
{
    public interface IPlaylistItemFactory : IStandardFactory
    {
        PlaylistItem Create(string fileName);

        PlaylistItem Create(LibraryItem libraryItem);
    }
}
