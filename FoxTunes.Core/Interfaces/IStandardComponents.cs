namespace FoxTunes.Interfaces
{
    public interface IStandardComponents
    {
        IConfiguration Configuration { get; }

        IDatabase Database { get; }

        IUserInterface UserInterface { get; }

        IPlaylist Playlist { get; }

        IOutput Output { get; }

        IScriptingRuntime ScriptingRuntime { get; }
    }
}
