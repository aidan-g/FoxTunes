namespace FoxTunes.Interfaces
{
    public interface ICoreScripts : IBaseComponent
    {
        string Artist { get; }

        string Artist_Album { get; }

        string Disk_Track_Title { get; }

        string Duration { get; }

        string Codec { get; }

        string Genre { get; }

        string Title_Performer { get; }

        string Track { get; }

        string Year_Album { get; }

        string ReplayGainAlbumGain { get; }

        string ReplayGainAlbumPeak { get; }

        string ReplayGainTrackGain { get; }

        string ReplayGainTrackPeak { get; }

        string PlayCount { get; }

        string LastPlayed { get; }
    }
}
