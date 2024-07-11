namespace FoxTunes.Interfaces
{
    public interface IMetaDataProviderManager : IStandardComponent, IDatabaseInitializer
    {
        IMetaDataProvider GetProvider(MetaDataProvider metaDataProvider);

        MetaDataProvider[] GetProviders();
    }
}
