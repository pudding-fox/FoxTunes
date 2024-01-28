namespace FoxTunes.Interfaces
{
    public interface IOutput : IStandardComponent
    {
        bool IsSupported(string fileName);

        IOutputStream Load(string fileName);

        void Unload(IOutputStream stream);
    }
}
