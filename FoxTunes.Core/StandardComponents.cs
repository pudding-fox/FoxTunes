using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class StandardComponents : IStandardComponents
    {
        private StandardComponents()
        {

        }

        public IConfiguration Configuration
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IConfiguration>();
            }
        }

        public IDatabase Database
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IDatabase>();
            }
        }

        public IUserInterface UserInterface
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IUserInterface>();
            }
        }

        public IPlaylist Playlist
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IPlaylist>();
            }
        }

        public ILibrary Library
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<ILibrary>();
            }
        }

        public IOutput Output
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IOutput>();
            }
        }

        public IScriptingRuntime ScriptingRuntime
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IScriptingRuntime>();
            }
        }

        public static readonly IStandardComponents Instance = new StandardComponents();
    }
}
