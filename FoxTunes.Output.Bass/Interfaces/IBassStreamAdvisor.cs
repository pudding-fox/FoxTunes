namespace FoxTunes.Interfaces
{
    public interface IBassStreamAdvisor : IBaseComponent
    {
        byte Priority { get; }

        bool Advice(IBassStreamProvider provider, PlaylistItem playlistItem, out IBassStreamAdvice Advice);
    }
}
