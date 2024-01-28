namespace FoxTunes.Interfaces
{
    public interface IStandardFactories
    {
        IDatabaseFactory Database { get; }

        IMetaDataSourceFactory MetaDataSource { get; }
    }
}
