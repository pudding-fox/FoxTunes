namespace FoxTunes.Interfaces
{
    public interface IDatabase : IStandardComponent
    {
        IDatabaseContext CreateContext();
    }
}
