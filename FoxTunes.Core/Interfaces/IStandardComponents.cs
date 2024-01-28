namespace FoxTunes.Interfaces
{
    public interface IStandardComponents
    {
        IConfiguration Configuration { get; }

        IDatabase Database { get; }

        IUserInterface UserInterface { get; }

        IPlaylist Playlist { get; }

        ILibrary Library { get; }

        IOutput Output { get; }

        IOutputStreamQueue OutputStreamQueue { get; }

        IScriptingRuntime ScriptingRuntime { get; }

        IBackgroundTaskRunner BackgroundTaskRunner { get; }

        IForegroundTaskRunner ForegroundTaskRunner { get; }

        ILogger Logger { get; }

        ILogEmitter LogEmitter { get; }

        ISignalEmitter SignalEmitter { get; }
    }
}
