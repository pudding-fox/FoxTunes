using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class MetaDataSourceFactory : StandardFactory, IMetaDataSourceFactory
    {
        public abstract bool Enabled { get; }

        public abstract IMetaDataSource Create();
    }
}
