namespace FoxTunes.Interfaces
{
    public interface IBassStreamFactory : IBaseComponent
    {
        void Register(IBassStreamProvider provider);

        int CreateStream(PlaylistItem playlistItem);
    }
}
