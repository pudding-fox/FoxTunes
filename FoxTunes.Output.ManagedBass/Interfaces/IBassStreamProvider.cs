namespace FoxTunes.Interfaces
{
    public interface IBassStreamProvider
    {
        byte Priority { get; }

        bool CanCreateStream(IBassOutput output, PlaylistItem playlistItem);

        int CreateStream(IBassOutput output, PlaylistItem playlistItem);
    }
}
