using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistCache : IStandardComponent
    {
        PlaylistColumn[] GetColumns(Func<IEnumerable<PlaylistColumn>> factory);

        Playlist[] GetPlaylists(Func<IEnumerable<Playlist>> factory);

        PlaylistItem[] GetItems(Playlist playlist, Func<IEnumerable<PlaylistItem>> factory);

        bool TryGetItemById(int id, out PlaylistItem playlistItem);

        bool TryGetItemBySequence(Playlist playlist, int sequence, out PlaylistItem playlistItem);

        bool TryGetItemsByLibraryId(int id, out PlaylistItem[] playlistItems);
    }
}
