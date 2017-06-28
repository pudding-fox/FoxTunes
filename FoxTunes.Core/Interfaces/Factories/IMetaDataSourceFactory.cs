namespace FoxTunes.Interfaces
{
    public interface IMetaDataSourceFactory : IStandardFactory
    {
        IMetaDataSource Create(string fileName);
    }
}
