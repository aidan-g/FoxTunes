using FoxDb.Interfaces;

namespace FoxTunes.Interfaces
{
    public interface IDatabaseQueries : IBaseComponent
    {
        IDatabaseQuery AddLibraryHierarchyNode { get; }

        IDatabaseQuery AddLibraryHierarchyNodeToPlaylist(string filter, string sort);

        IDatabaseQuery AddLibraryMetaDataItem { get; }

        IDatabaseQuery AddPlaylistMetaDataItem { get; }

        IDatabaseQuery ClearLibraryMetaDataItems { get; }

        IDatabaseQuery ClearPlaylistMetaDataItems { get; }

        IDatabaseQuery GetLibraryMetaData { get; }

        IDatabaseQuery GetLibraryHierarchyMetaData(string filter);

        IDatabaseQuery GetLibraryHierarchyNodes(string filter);

        IDatabaseQuery GetLibraryItems(string filter);

        IDatabaseQuery GetOrAddMetaDataItem { get; }

        IDatabaseQuery GetPlaylistMetaData(int count);

        IDatabaseQuery MovePlaylistItem { get; }

        IDatabaseQuery RemoveCancelledLibraryItems { get; }

        IDatabaseQuery RemoveLibraryHierarchyItems { get; }

        IDatabaseQuery RemoveLibraryItems { get; }

        IDatabaseQuery RemovePlaylistItems { get; }

        IDatabaseQuery SequencePlaylistItems(string sort);

        IDatabaseQuery UpdateLibraryHierarchyNode { get; }

        IDatabaseQuery UpdateLibraryVariousArtists { get; }

        IDatabaseQuery UpdatePlaylistVariousArtists { get; }
    }
}
