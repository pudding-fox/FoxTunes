using CSCore.SoundOut;

namespace FoxTunes
{
    public class DirectSoundOutFactory : SoundOutFactory
    {
        public override ISoundOut CreateSoundOut()
        {
            return new DirectSoundOut();
        }
    }
}
