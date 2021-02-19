using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace FoxTunes
{
    [Component(ID, ComponentSlots.None)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ChartLyricsProvider : LyricsProvider, IConfigurableComponent
    {
        public const string ID = "2C54A300-DD63-49BD-B709-DAE6F6C10018";

        public const string BASE_URL = "http://api.chartlyrics.com/apiv1.asmx";

        public ChartLyricsProvider() : base(ID, "Chart Lyrics")
        {

        }

        public IConfiguration Configuration { get; private set; }

        public TextConfigurationElement BaseUrl { get; private set; }

        public DoubleConfigurationElement MinConfidence { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.BaseUrl = this.Configuration.GetElement<TextConfigurationElement>(
                ChartLyricsProviderConfiguration.SECTION,
                ChartLyricsProviderConfiguration.BASE_URL
            );
            this.MinConfidence = this.Configuration.GetElement<DoubleConfigurationElement>(
                ChartLyricsProviderConfiguration.SECTION,
                ChartLyricsProviderConfiguration.MIN_CONFIDENCE
            );
            base.InitializeComponent(core);
        }

        public override async Task<LyricsResult> Lookup(IFileData fileData)
        {
            Logger.Write(this, LogLevel.Debug, "Getting track information for file \"{0}\"..", fileData.FileName);
            var artist = default(string);
            var song = default(string);
            if (!this.TryGetLookup(fileData, out artist, out song))
            {
                Logger.Write(this, LogLevel.Warn, "Failed to get track information: The required meta data was not found.");
                return LyricsResult.Fail;
            }
            Logger.Write(this, LogLevel.Debug, "Got track information: Artist = \"{0}\", Song = \"{1}\".", artist, song);
            try
            {
                Logger.Write(this, LogLevel.Debug, "Searching for match..");
                var searchResult = await this.Lookup(artist, song).ConfigureAwait(false);
                if (searchResult != null)
                {
                    Logger.Write(this, LogLevel.Debug, "Got match, fetching lyrics..");
                    var lyricsResult = await this.Lookup(searchResult).ConfigureAwait(false);
                    if (lyricsResult != null && !string.IsNullOrEmpty(lyricsResult.Lyric))
                    {
                        Logger.Write(this, LogLevel.Debug, "Success.");
                        return new LyricsResult(lyricsResult.Lyric);
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: Warn.
            }
            return LyricsResult.Fail;
        }

        protected virtual async Task<SearchLyricResult> Lookup(string artist, string song)
        {
            var baseUrl = this.BaseUrl.Value;
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = BASE_URL;
            }
            var url = string.Format(
                "{0}/SearchLyric?artist={1}&song={2}",
                baseUrl,
                Uri.EscapeDataString(artist),
                Uri.EscapeDataString(song)
            );
            Logger.Write(this, LogLevel.Debug, "Querying the API: {0}", url);
            var request = WebRequest.Create(url);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Logger.Write(this, LogLevel.Warn, "Status code does not indicate success.");
                    return null;
                }
                using (var stream = response.GetResponseStream())
                {
                    var results = Serializer.LoadSearchLyricResults(stream).Where(
                        result => result.Valid
                    );
                    return await this.Lookup(artist, song, results).ConfigureAwait(false);
                }
            }
        }

        protected virtual Task<SearchLyricResult> Lookup(string artist, string song, IEnumerable<SearchLyricResult> searchResults)
        {
            var searchResult = searchResults.ToDictionary(
                //Map results to similarity
                _searchResult => _searchResult,
                _searchResult => _searchResult.Similarity(artist, song)
            ).Where(
                //Where they have the required confidence.
                _pair => _pair.Value >= this.MinConfidence.Value
            ).OrderByDescending(
                //Order by highest confidence first.
                _pair => _pair.Value
            ).Select(
                //Select result.
                _pair => _pair.Key
            ).FirstOrDefault();
#if NET40
            return TaskEx.FromResult(searchResult);
#else
            return Task.FromResult(searchResult);
#endif
        }

        protected virtual Task<GetLyricResult> Lookup(SearchLyricResult searchResult)
        {
            var baseUrl = this.BaseUrl.Value;
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = BASE_URL;
            }
            var url = string.Format(
                "{0}/GetLyric?lyricId={1}&lyricChecksum={2}",
                baseUrl,
                Uri.EscapeDataString(searchResult.LyricId),
                Uri.EscapeDataString(searchResult.LyricChecksum)
            );
            Logger.Write(this, LogLevel.Debug, "Querying the API: {0}", url);
            var request = WebRequest.Create(url);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Logger.Write(this, LogLevel.Warn, "Status code does not indicate success.");
#if NET40
                    return TaskEx.FromResult(default(GetLyricResult));
#else
                    return Task.FromResult(default(GetLyricResult));
#endif
                }
                using (var stream = response.GetResponseStream())
                {
                    var result = Serializer.LoadGetLyricResult(stream);
#if NET40
                    return TaskEx.FromResult(result);
#else
                    return Task.FromResult(result);
#endif
                }
            }
        }

        protected virtual bool TryGetLookup(IFileData fileData, out string artist, out string song)
        {
            artist = default(string);
            song = default(string);
            lock (fileData.MetaDatas)
            {
                foreach (var metaDataItem in fileData.MetaDatas)
                {
                    if (string.Equals(metaDataItem.Name, CommonMetaData.Artist, StringComparison.OrdinalIgnoreCase))
                    {
                        artist = metaDataItem.Value;
                    }
                    else if (string.Equals(metaDataItem.Name, CommonMetaData.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        song = metaDataItem.Value;
                    }
                    if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(song))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return ChartLyricsProviderConfiguration.GetConfigurationSections();
        }

        public static class Serializer
        {
            public static IEnumerable<SearchLyricResult> LoadSearchLyricResults(Stream stream)
            {
                var results = new List<SearchLyricResult>();
                using (var reader = new XmlTextReader(stream))
                {
                    reader.WhitespaceHandling = WhitespaceHandling.Significant;
                    if (reader.IsStartElement(SearchLyricResult.ArrayOfSearchLyricResult))
                    {
                        reader.ReadStartElement(SearchLyricResult.ArrayOfSearchLyricResult);
                        while (reader.IsStartElement(nameof(SearchLyricResult)))
                        {
                            reader.ReadStartElement(nameof(SearchLyricResult));
                            results.Add(LoadSearchLyricResult(reader));
                            if (reader.NodeType == XmlNodeType.EndElement)
                            {
                                reader.ReadEndElement();
                            }
                        }
                    }
                }
                return results;
            }

            public static GetLyricResult LoadGetLyricResult(Stream stream)
            {
                using (var reader = new XmlTextReader(stream))
                {
                    reader.WhitespaceHandling = WhitespaceHandling.Significant;
                    if (reader.IsStartElement(nameof(GetLyricResult)))
                    {
                        reader.ReadStartElement(nameof(GetLyricResult));
                        try
                        {
                            return LoadGetLyricResult(reader);
                        }
                        finally
                        {
                            if (reader.NodeType == XmlNodeType.EndElement)
                            {
                                reader.ReadEndElement();
                            }
                        }
                    }
                }
                return null;
            }

            private static SearchLyricResult LoadSearchLyricResult(XmlTextReader reader)
            {
                var result = new SearchLyricResult();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.IsStartElement())
                    {
                        if (string.Equals(reader.Name, nameof(SearchLyricResult.TrackChecksum), StringComparison.OrdinalIgnoreCase))
                        {
                            result.TrackChecksum = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(SearchLyricResult.TrackId), StringComparison.OrdinalIgnoreCase))
                        {
                            result.TrackId = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(SearchLyricResult.LyricChecksum), StringComparison.OrdinalIgnoreCase))
                        {
                            result.LyricChecksum = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(SearchLyricResult.LyricId), StringComparison.OrdinalIgnoreCase))
                        {
                            result.LyricId = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(SearchLyricResult.SongUrl), StringComparison.OrdinalIgnoreCase))
                        {
                            result.SongUrl = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(SearchLyricResult.ArtistUrl), StringComparison.OrdinalIgnoreCase))
                        {
                            result.ArtistUrl = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(SearchLyricResult.Artist), StringComparison.OrdinalIgnoreCase))
                        {
                            result.Artist = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(SearchLyricResult.Song), StringComparison.OrdinalIgnoreCase))
                        {
                            result.Song = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(SearchLyricResult.SongRank), StringComparison.OrdinalIgnoreCase))
                        {
                            result.SongRank = Read(reader);
                        }
                    }
                }
                return result;
            }

            private static GetLyricResult LoadGetLyricResult(XmlTextReader reader)
            {
                var result = new GetLyricResult();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.IsStartElement())
                    {
                        if (string.Equals(reader.Name, nameof(GetLyricResult.TrackChecksum), StringComparison.OrdinalIgnoreCase))
                        {
                            result.TrackChecksum = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(GetLyricResult.TrackId), StringComparison.OrdinalIgnoreCase))
                        {
                            result.TrackId = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(GetLyricResult.LyricChecksum), StringComparison.OrdinalIgnoreCase))
                        {
                            result.LyricChecksum = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(GetLyricResult.LyricId), StringComparison.OrdinalIgnoreCase))
                        {
                            result.LyricId = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(GetLyricResult.LyricSong), StringComparison.OrdinalIgnoreCase))
                        {
                            result.LyricSong = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(GetLyricResult.LyricArtist), StringComparison.OrdinalIgnoreCase))
                        {
                            result.LyricArtist = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(GetLyricResult.LyricUrl), StringComparison.OrdinalIgnoreCase))
                        {
                            result.LyricUrl = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(GetLyricResult.LyricCovertArtUrl), StringComparison.OrdinalIgnoreCase))
                        {
                            result.LyricCovertArtUrl = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(GetLyricResult.LyricRank), StringComparison.OrdinalIgnoreCase))
                        {
                            result.LyricRank = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(GetLyricResult.LyricCorrectUrl), StringComparison.OrdinalIgnoreCase))
                        {
                            result.LyricCorrectUrl = Read(reader);
                        }
                        else if (string.Equals(reader.Name, nameof(GetLyricResult.Lyric), StringComparison.OrdinalIgnoreCase))
                        {
                            result.Lyric = Read(reader);
                        }
                    }
                }
                return result;
            }

            private static string Read(XmlTextReader reader)
            {
                reader.ReadStartElement();
                try
                {
                    return reader.ReadContentAsString();
                }
                finally
                {
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        reader.ReadEndElement();
                    }
                }
            }
        }

        public class SearchLyricResult
        {
            public const string ArrayOfSearchLyricResult = "ArrayOfSearchLyricResult";

            public string TrackChecksum { get; set; }

            public string TrackId { get; set; }

            public string LyricChecksum { get; set; }

            public string LyricId { get; set; }

            public string SongUrl { get; set; }

            public string ArtistUrl { get; set; }

            public string Artist { get; set; }

            public string Song { get; set; }

            public string SongRank { get; set; }

            public bool Valid
            {
                get
                {
                    var valid = true;
                    //Required by GetLyric.
                    if (string.IsNullOrEmpty(this.LyricChecksum))
                    {
                        valid = false;
                    }
                    else if (string.IsNullOrEmpty(this.LyricId))
                    {
                        valid = false;
                    }
                    else if (string.Equals(this.LyricId, "0", StringComparison.OrdinalIgnoreCase))
                    {
                        valid = false;
                    }
                    //Required by similarity check.
                    else if (string.IsNullOrEmpty(this.Artist))
                    {
                        valid = false;
                    }
                    else if (string.IsNullOrEmpty(this.Song))
                    {
                        valid = false;
                    }
                    Logger.Write(typeof(ChartLyricsProvider), LogLevel.Trace, "Lyric search: LyricChecksum = {0}, LyricId = {1}, Artist = \"{2}\", Song = \"{3}\", Valid = {4}.", this.LyricChecksum, this.LyricId, this.Artist, this.Song, valid);
                    return valid;
                }
            }

            public float Similarity(string artist, string song)
            {
                var similarity = (this.Artist.Similarity(artist, true) + this.Song.Similarity(song, true)) / 2;
                Logger.Write(typeof(ChartLyricsProvider), LogLevel.Trace, "Lyric search: LyricChecksum = {0}, LyricId = {1}, Confidence = {2}.", this.LyricChecksum, this.LyricId, similarity);
                return similarity;
            }
        }

        public class GetLyricResult
        {
            public string TrackChecksum { get; set; }

            public string TrackId { get; set; }

            public string LyricChecksum { get; set; }

            public string LyricId { get; set; }

            public string LyricSong { get; set; }

            public string LyricArtist { get; set; }

            public string LyricUrl { get; set; }

            public string LyricCovertArtUrl { get; set; }

            public string LyricRank { get; set; }

            public string LyricCorrectUrl { get; set; }

            public string Lyric { get; set; }
        }
    }
}
