using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class CueSheetParser : BaseComponent
    {
        public const string REM = "REM";

        public const string TITLE = "TITLE";

        public const string PERFORMER = "PERFORMER";

        public const string SONGWRITER = "SONGWRITER";

        public const string ISRC = "ISRC";

        public const string FILE = "FILE";

        public const string TRACK = "TRACK";

        public const string FLAGS = "FLAGS";

        public const string INDEX = "INDEX";

        public CueSheet Parse(string fileName)
        {
            var lines = File.ReadAllLines(fileName);
            return this.Parse(fileName, lines);
        }

        public CueSheet Parse(string fileName, string[] lines)
        {
            var files = new List<CueSheetFile>();
            var tags = new List<CueSheetTag>();
            this.OnParse(lines, files, tags);
            return new CueSheet(fileName, files, tags);
        }

        protected virtual void OnParse(string[] lines, IList<CueSheetFile> files, IList<CueSheetTag> tags)
        {
            for (var position = 0; position < lines.Length; position++)
            {
                this.OnParse(lines, ref position, files, tags);
            }
        }

        protected virtual void OnParse(string[] lines, ref int position, IList<CueSheetFile> files, IList<CueSheetTag> tags)
        {
            var line = lines[position].Trim();
            var parts = line.Split(new[] { ' ' }, 2);
            if (parts.Length != 2)
            {
                return;
            }
            switch (parts[0].ToUpper())
            {
                case REM:
                    this.OnParseTag(lines, ref position, files, tags);
                    break;
                case TITLE:
                case PERFORMER:
                case SONGWRITER:
                    this.OnParseTag(lines, ref position, files, tags);
                    break;
                case FILE:
                    this.OnParseFile(lines, ref position, files, tags);
                    break;
            }
        }

        private void OnParseFile(string[] lines, ref int position, IList<CueSheetFile> files, IList<CueSheetTag> tags)
        {
            var line = lines[position].Trim();
            var path = line.Substring(
                line.IndexOf(' '),
                line.LastIndexOf(' ') - line.IndexOf(' ')
            ).Trim(new[] { '"', ' ' });
            var format = line.Substring(
                line.LastIndexOf(' ')
            ).Trim();
            var tracks = new List<CueSheetTrack>();
            var trackNumber = default(string);
            var trackType = default(string);
            var trackIndexes = new List<CueSheetIndex>();
            var trackTags = new List<CueSheetTag>();
            for (position = position + 1; position < lines.Length; position++)
            {
                line = lines[position].Trim();
                var parts = line.Split(new[] { ' ' }, 3);
                if (parts.Length < 2)
                {
                    continue;
                }
                switch (parts[0].ToUpper())
                {
                    case REM:
                        this.OnParseTag(lines, ref position, files, trackTags);
                        break;
                    case TITLE:
                    case PERFORMER:
                    case SONGWRITER:
                        this.OnParseTag(lines, ref position, files, trackTags);
                        break;
                    case ISRC:
                        //TODO: Should we do something with this? International Standard Recording Code.
                        break;
                    case FLAGS:
                        //TODO: Should we do something with this? Copy protection, multi channel...
                        break;
                    case TRACK:
                        if (!string.IsNullOrEmpty(trackNumber) && !string.IsNullOrEmpty(trackType) && trackIndexes.Count > 0)
                        {
                            var track = new CueSheetTrack(
                                trackNumber,
                                trackType,
                                trackIndexes,
                                trackTags
                            );
                            tracks.Add(track);
                        }
                        trackNumber = default(string);
                        trackType = default(string);
                        trackIndexes = new List<CueSheetIndex>();
                        trackTags = new List<CueSheetTag>();
                        if (parts.Length == 3)
                        {
                            trackNumber = parts[1];
                            trackType = parts[2];
                        }
                        break;
                    case INDEX:
                        if (parts.Length == 3)
                        {
                            var indexPosition = parts[1];
                            var indexTime = parts[2];
                            var index = new CueSheetIndex(
                                indexPosition,
                                indexTime
                            );
                            trackIndexes.Add(index);
                        }
                        break;
                    default:
                        position--;
                        goto done;
                }
            }
        done:
            if (!string.IsNullOrEmpty(trackNumber) && !string.IsNullOrEmpty(trackType) && trackIndexes.Count > 0)
            {
                var track = new CueSheetTrack(
                    trackNumber,
                    trackType,
                    trackIndexes,
                    trackTags
                );
                tracks.Add(track);
            }
            var file = new CueSheetFile(path, format, tracks);
            files.Add(file);
        }

        private void OnParseTag(string[] lines, ref int position, IList<CueSheetFile> files, IList<CueSheetTag> tags)
        {
            var line = lines[position].Trim();
            var parts = line.Split(' ').ToList();
            if (parts.Count < 2)
            {
                return;
            }
            if (string.Equals(parts[0], REM, StringComparison.OrdinalIgnoreCase))
            {
                parts.RemoveAt(0);
            }
            var name = parts[0];
            var value = string.Join(" ", parts.Skip(1)).Trim(new[] { '"', ' ' });
            tags.Add(new CueSheetTag(name, value));
        }
    }
}
