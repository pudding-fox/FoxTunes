namespace FoxTunes.Interfaces
{
    public interface IStandardComponents
    {
        IConfiguration Configuration { get; }

        IUserInterface UserInterface { get; }

        IOutput Output { get; }

        IOutputStreamQueue OutputStreamQueue { get; }

        IScriptingRuntime ScriptingRuntime { get; }

        IBackgroundTaskRunner BackgroundTaskRunner { get; }

        IForegroundTaskRunner ForegroundTaskRunner { get; }

        ILogger Logger { get; }

        ISignalEmitter SignalEmitter { get; }

        ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; }

        IArtworkProvider ArtworkProvider { get; }
    }
}
