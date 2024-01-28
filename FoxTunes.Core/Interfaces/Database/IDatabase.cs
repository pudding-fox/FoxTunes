namespace FoxTunes.Interfaces
{
    public interface IDatabase : IStandardComponent
    {
        ICoreSQL CoreSQL { get; }

        IDatabaseContext CreateContext();
    }
}
