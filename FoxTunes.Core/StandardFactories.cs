using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class StandardFactories : IStandardFactories
    {
        public IDatabaseFactory Database
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IDatabaseFactory>();
            }
        }

        public IMetaDataSourceFactory MetaDataSource
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IMetaDataSourceFactory>();
            }
        }

        public static readonly IStandardFactories Instance = new StandardFactories();
    }
}
