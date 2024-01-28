namespace FoxTunes.Interfaces
{
    public interface ICoreValidator : IBaseComponent
    {
        bool Validate(ICore core);
    }
}
