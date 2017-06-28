using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class BehaviourLoader : BaseLoader<IStandardBehaviour>
    {
        private BehaviourLoader()
        {

        }

        public static readonly IBaseLoader<IStandardBehaviour> Instance = new BehaviourLoader();
    }
}
