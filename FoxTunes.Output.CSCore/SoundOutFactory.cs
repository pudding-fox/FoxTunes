using CSCore.SoundOut;

namespace FoxTunes
{
    public abstract class SoundOutFactory : BaseComponent, ISoundOutFactory
    {
        public abstract ISoundOut CreateSoundOut();
    }
}
