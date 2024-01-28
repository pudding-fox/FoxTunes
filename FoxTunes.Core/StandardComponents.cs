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

        public IOutput Output
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IOutput>();
            }
        }

        public IOutputStreamQueue OutputStreamQueue
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IOutputStreamQueue>();
            }
        }

        public IScriptingRuntime ScriptingRuntime
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IScriptingRuntime>();
            }
        }

        public IBackgroundTaskRunner BackgroundTaskRunner
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IBackgroundTaskRunner>();
            }
        }

        public IForegroundTaskRunner ForegroundTaskRunner
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IForegroundTaskRunner>();
            }
        }

        public ILogger Logger
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<ILogger>();
            }
        }

        public ILogEmitter LogEmitter
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<ILogEmitter>();
            }
        }

        public ISignalEmitter SignalEmitter
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<ISignalEmitter>();
            }
        }

        public static readonly IStandardComponents Instance = new StandardComponents();
    }
}
