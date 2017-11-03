using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class JSCoreScripts : BaseComponent, ICoreScripts
    {
        public string PlaylistSortValues
        {
            get
            {
                return Resources.PlaylistSortValues;
            }
        }

        public static ICoreScripts Instance = new JSCoreScripts();
    }
}
