using System;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistBrowser : IStandardComponent
    {
        PlaylistBrowserState State { get; }

        event EventHandler StateChanged;

        PlaylistColumn[] GetColumns();

        Playlist[] GetPlaylists();

        Playlist GetPlaylist(PlaylistItem playlistItem);

        PlaylistItem[] GetItems(Playlist playlist);

        PlaylistItem GetItemById(Playlist playlist, int id);

        PlaylistItem GetItemBySequence(Playlist playlist, int sequence);

        PlaylistItem GetFirstItem(Playlist playlist);

        PlaylistItem GetLastItem(Playlist playlist);

        PlaylistItem GetNextItem(Playlist playlist);

        PlaylistItem GetNextItem(PlaylistItem playlistItem);

        PlaylistItem GetPreviousItem(Playlist playlist);

        PlaylistItem GetPreviousItem(PlaylistItem playlistItem);

        int GetInsertIndex(Playlist playlist);
    }

    public enum PlaylistBrowserState : byte
    {
        None,
        Loading
    }
}
