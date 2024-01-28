namespace FoxTunes.Interfaces
{
    public interface IPlaylistItemFactory : IStandardFactory
    {
        PlaylistItem Create(int sequence, string fileName);

        PlaylistItem Create(int sequence, LibraryItem libraryItem);
    }
}
