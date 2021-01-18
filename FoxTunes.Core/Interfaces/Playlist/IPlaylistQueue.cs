namespace FoxTunes.Interfaces
{
    public interface IPlaylistQueue : IStandardComponent, IInvocableComponent
    {
        PlaylistItem GetNext(PlaylistItem playlistItem);

        int GetPosition(PlaylistItem playlistItem);
    }
}
