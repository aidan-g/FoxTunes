using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IPlaylistManager : IStandardManager, IBackgroundTaskSource, IInvocableComponent, IFileActionHandler, IDatabaseInitializer
    {
        Task Add(Playlist playlist);

        Task Add(Playlist playlist, IEnumerable<string> paths);

        Task Add(Playlist playlist, LibraryHierarchyNode libraryHierarchyNode);

        Task Add(Playlist playlist, IEnumerable<PlaylistItem> playlistItems);

        Task Remove(Playlist playlist);

        Task Add(Playlist playlist, IEnumerable<string> paths, bool clear);

        Task Insert(Playlist playlist, int index, IEnumerable<string> paths, bool clear);

        Task Add(Playlist playlist, LibraryHierarchyNode libraryHierarchyNode, bool clear);

        Task Insert(Playlist playlist, int index, LibraryHierarchyNode libraryHierarchyNode, bool clear);

        Task Add(Playlist playlist, IEnumerable<LibraryHierarchyNode> libraryHierarchyNodes, bool clear);

        Task Insert(Playlist playlist, int index, IEnumerable<LibraryHierarchyNode> libraryHierarchyNodes, bool clear);

        Task Move(Playlist playlist, IEnumerable<PlaylistItem> playlistItems);

        Task Move(Playlist playlist, int index, IEnumerable<PlaylistItem> playlistItems);

        Task Remove(Playlist playlist, IEnumerable<PlaylistItem> playlistItems);

        Task Crop(Playlist playlist, IEnumerable<PlaylistItem> playlistItems);

        Task Play(PlaylistItem playlistItem);

        Task Play(Playlist playlist, int sequence);

        Task Next();

        Task Previous();

        Task Clear(Playlist playlist);

        Task Sort(Playlist playlist, PlaylistColumn playlistColumn);

        Playlist SelectedPlaylist { get; set; }

        event EventHandler SelectedPlaylistChanged;

        Playlist CurrentPlaylist { get; }

        event EventHandler CurrentPlaylistChanged;

        PlaylistItem CurrentItem { get; }

        event EventHandler CurrentItemChanged;

        PlaylistItem[] SelectedItems { get; set; }

        event EventHandler SelectedItemsChanged;
    }
}
