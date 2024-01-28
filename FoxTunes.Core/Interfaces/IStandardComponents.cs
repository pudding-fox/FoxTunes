namespace FoxTunes.Interfaces
{
    public interface IStandardComponents
    {
        IConfiguration Configuration { get; }

        IUserInterface UserInterface { get; }

        IOutput Output { get; }

        IOutputStreamQueue OutputStreamQueue { get; }

        IScriptingRuntime ScriptingRuntime { get; }

        ISignalEmitter SignalEmitter { get; }

        ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; }

        ILibraryHierarchyCache LibraryHierarchyCache { get; }

        IArtworkProvider ArtworkProvider { get; }

        IFileSystemBrowser FileSystemBrowser { get; }

        IMetaDataBrowser MetaDataBrowser { get; }

        IMetaDataCache MetaDataCache { get; }

        IPlaylistBrowser PlaylistBrowser { get; }

        IPlaylistCache PlaylistCache { get; }

        IFilterParser FilterParser { get; }
    }
}
