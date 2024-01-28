using FoxTunes.Interfaces;

namespace FoxTunes
{
    public interface IBassEncoderFactory : IStandardComponent
    {
        IBassEncoder CreateEncoder(int concurrency);
    }
}
