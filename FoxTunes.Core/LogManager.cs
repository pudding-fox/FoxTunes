using FoxTunes.Interfaces;

namespace FoxTunes
{
    public static class LogManager
    {
        public static ILogger Logger
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<ILogger>();
            }
        }
    }
}
