using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class StandardManagers : IStandardManagers
    {
        private StandardManagers()
        {

        }

        public IPlaybackManager Playback
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IPlaybackManager>();
            }
        }

        public IPlaylistManager Playlist
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IPlaylistManager>();
            }
        }

        public ILibraryManager Library
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<ILibraryManager>();
            }
        }

        public IHierarchyManager Hierarchy
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IHierarchyManager>();
            }
        }

        public IMetaDataManager MetaData
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IMetaDataManager>();
            }
        }

        public IFileActionHandlerManager FileActionHandler
        {
            get
            {
                return ComponentRegistry.Instance.GetComponent<IFileActionHandlerManager>();
            }
        }

        public static readonly IStandardManagers Instance = new StandardManagers();
    }
}
