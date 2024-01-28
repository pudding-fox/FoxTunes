namespace FoxTunes.Interfaces
{
    public interface IStandardManagers
    {
        IPlaybackManager Playback { get; }

        IPlaylistManager Playlist { get; }
    }
}
