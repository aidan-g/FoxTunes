namespace FoxTunes.Interfaces
{
    public interface IStandardManagers
    {
        IPlaybackManager Playback { get; }

        IPlaylistManager Playlist { get; }

        ILibraryManager Library { get; }

        IHierarchyManager Hierarchy { get; }

        IMetaDataManager MetaData { get; }

        IFileActionHandlerManager FileActionHandler { get; }
    }
}
