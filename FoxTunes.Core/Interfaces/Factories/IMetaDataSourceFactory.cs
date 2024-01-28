namespace FoxTunes.Interfaces
{
    public interface IMetaDataSourceFactory : IStandardFactory
    {
        bool Enabled { get; }

        IMetaDataSource Create();
    }
}
