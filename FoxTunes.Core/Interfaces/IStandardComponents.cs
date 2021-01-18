namespace FoxTunes.Interfaces
{
    public interface IStandardComponents
    {
        IConfiguration Configuration { get; }

        IUserInterface UserInterface { get; }

        IOutput Output { get; }

        IOutputEffects OutputEffects { get; }

        IOutputStreamQueue OutputStreamQueue { get; }

        IScriptingRuntime ScriptingRuntime { get; }

        ISignalEmitter SignalEmitter { get; }

        ILibraryBrowser LibraryBrowser { get; }

        ILibraryCache LibraryCache { get; }

        ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; }

        ILibraryHierarchyCache LibraryHierarchyCache { get; }

        IArtworkProvider ArtworkProvider { get; }

        IFileSystemBrowser FileSystemBrowser { get; }

        IMetaDataBrowser MetaDataBrowser { get; }

        IMetaDataCache MetaDataCache { get; }

        IMetaDataSynchronizer MetaDataSynchronizer { get; }

        IPlaylistBrowser PlaylistBrowser { get; }

        IPlaylistCache PlaylistCache { get; }

        IPlaylistQueue PlaylistQueue { get; }

        IFilterParser FilterParser { get; }

        ISortParser SortParser { get; }
    }
}
