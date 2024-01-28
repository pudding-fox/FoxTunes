namespace FoxTunes.Interfaces
{
    public interface IBassStreamProvider
    {
        byte Priority { get; }

        bool CanCreateStream(PlaylistItem playlistItem);

        int CreateStream(PlaylistItem playlistItem);
    }
}
