namespace FoxTunes.Interfaces
{
    public interface IPlaylistQueue : IStandardComponent, IInvocableComponent
    {
        int GetQueuePosition(PlaylistItem playlistItem);
    }
}
