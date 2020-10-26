using System;
using System.Collections.Generic;
using System.Reflection;

namespace FoxTunes
{
    public static class CommonProperties
    {
        public const string AudioBitrate = "AudioBitrate";
        public const string AudioChannels = "AudioChannels";
        public const string AudioSampleRate = "AudioSampleRate";
        public const string BitsPerSample = "BitsPerSample";
        public const string Description = "Description";
        public const string Duration = "Duration";
        public const string PhotoHeight = "PhotoHeight";
        public const string PhotoQuality = "PhotoQuality";
        public const string PhotoWidth = "PhotoWidth";
        public const string VideoHeight = "VideoHeight";
        public const string VideoWidth = "VideoWidth";

        public static IDictionary<string, string> Lookup = GetLookup();

        private static IDictionary<string, string> GetLookup()
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in typeof(CommonProperties).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var name = field.GetValue(null) as string;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                lookup.Add(name, name);
            }
            return lookup;
        }
    }
}
