namespace FoxTunes.Interfaces
{
    public interface IPlaylist : IBaseComponent
    {
        IDatabaseQuery<PlaylistItem> Query { get; }

        IDatabaseSet<PlaylistItem> Set { get; }
    }
}
