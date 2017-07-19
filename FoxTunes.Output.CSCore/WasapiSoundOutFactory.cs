using CSCore.SoundOut;

namespace FoxTunes
{
    public class WasapiSoundOutFactory : SoundOutFactory
    {
        public override ISoundOut CreateSoundOut()
        {
            return new WasapiOut();
        }
    }
}
