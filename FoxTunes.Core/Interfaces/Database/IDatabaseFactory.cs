namespace FoxTunes.Interfaces
{
    public interface IDatabaseFactory : IStandardFactory
    {
        bool Test();

        void Initialize();

        IDatabaseComponent Create();
    }
}
