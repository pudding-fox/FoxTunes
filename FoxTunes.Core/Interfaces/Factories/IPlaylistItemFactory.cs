namespace FoxTunes.Interfaces
{
    public interface IPlaylistItemFactory : IStandardFactory
    {
        IPlaylistItem Create(string fileName);
    }
}
