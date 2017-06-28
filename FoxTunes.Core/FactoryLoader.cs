using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class FactoryLoader : BaseLoader<IStandardFactory>
    {
        private FactoryLoader()
        {

        }

        public static readonly IBaseLoader<IStandardFactory> Instance = new FactoryLoader();
    }
}
