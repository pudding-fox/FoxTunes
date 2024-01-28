namespace FoxTunes.Interfaces
{
    public interface IBassStreamAdvisor : IBaseComponent
    {
        byte Priority { get; }

        bool Advice(PlaylistItem playlistItem, out IBassStreamAdvice Advice);
    }
}
