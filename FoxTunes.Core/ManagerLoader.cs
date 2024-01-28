using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class ManagerLoader : BaseLoader<IStandardManager>
    {
        private ManagerLoader()
        {

        }

        public static readonly IBaseLoader<IStandardManager> Instance = new ManagerLoader();
    }
}
