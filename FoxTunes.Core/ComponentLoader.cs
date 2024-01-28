using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class ComponentLoader : BaseLoader<IStandardComponent>
    {
        private ComponentLoader()
        {

        }

        public static readonly IBaseLoader<IStandardComponent> Instance = new ComponentLoader();
    }
}
