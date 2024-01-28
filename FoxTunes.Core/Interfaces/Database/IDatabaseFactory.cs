namespace FoxTunes.Interfaces
{
    public interface IDatabaseFactory : IStandardFactory
    {
        IDatabaseComponent Create();
    }
}
