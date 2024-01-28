using CSCore.SoundOut;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    public interface ISoundOutFactory : IBaseComponent
    {
        ISoundOut CreateSoundOut();
    }
}
