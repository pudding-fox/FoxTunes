using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class MetaDataSourceFactory : StandardFactory, IMetaDataSourceFactory
    {
        public abstract IMetaDataSource Create();
    }
}
