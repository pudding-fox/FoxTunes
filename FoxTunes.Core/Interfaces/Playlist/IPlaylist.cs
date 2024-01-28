namespace FoxTunes.Interfaces
{
    public interface IPlaylist : IBaseComponent
    {
        IDatabaseSet<PlaylistItem> PlaylistItemSet { get; }

        IDatabaseQuery<PlaylistItem> PlaylistItemQuery { get; }

        IDatabaseSet<PlaylistColumn> PlaylistColumnSet { get; }

        IDatabaseQuery<PlaylistColumn> PlaylistColumnQuery { get; }
    }
}
