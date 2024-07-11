namespace FoxTunes.Interfaces
{
    public interface IOutputStreamDataSourceFactory : IStandardComponent
    {
        IOutputStreamDataSource Create(IOutputStream outputStream);
    }
}
