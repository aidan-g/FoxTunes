using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class JSCoreScripts : BaseComponent, ICoreScripts
    {
        public string Artist
        {
            get
            {
                return Resources.Artist;
            }
        }

        public string Artist_Album
        {
            get
            {
                return Resources.Artist_Album;
            }
        }

        public string Disk_Track_Title
        {
            get
            {
                return Resources.Disk_Track_Title;
            }
        }

        public string Duration
        {
            get
            {
                return Resources.Duration;
            }
        }

        public string Genre
        {
            get
            {
                return Resources.Genre;
            }
        }

        public string Codec
        {
            get
            {
                return Resources.Codec;
            }
        }

        public string Title_Performer
        {
            get
            {
                return Resources.Title_Performer;
            }
        }

        public string Track
        {
            get
            {
                return Resources.Track;
            }
        }

        public string Year_Album
        {
            get
            {
                return Resources.Year_Album;
            }
        }

        public string ReplayGainAlbumGain
        {
            get
            {
                return Resources.ReplayGainAlbumGain;
            }
        }

        public string ReplayGainAlbumPeak
        {
            get
            {
                return Resources.ReplayGainAlbumPeak;
            }
        }

        public string ReplayGainTrackGain
        {
            get
            {
                return Resources.ReplayGainTrackGain;
            }
        }

        public string ReplayGainTrackPeak
        {
            get
            {
                return Resources.ReplayGainTrackPeak;
            }
        }

        public string PlayCount
        {
            get
            {
                return Resources.PlayCount;
            }
        }

        public string LastPlayed
        {
            get
            {
                return Resources.LastPlayed;
            }
        }

        public static ICoreScripts Instance = new JSCoreScripts();
    }
}
