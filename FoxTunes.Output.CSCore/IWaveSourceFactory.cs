using CSCore;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public interface IWaveSourceFactory : IBaseComponent
    {
        bool IsSupported(string fileName);

        IWaveSource CreateWaveSource(string fileName);
    }
}
